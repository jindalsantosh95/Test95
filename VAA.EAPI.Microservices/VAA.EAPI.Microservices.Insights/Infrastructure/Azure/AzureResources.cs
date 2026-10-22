using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models;

namespace VAA.EAPI.Microservices.Insights.Infrastructure.Azure
{
    public class AzureResources(IHttpConnector httpConnector, ILogger<AzureResources> logger, IOAuthTokenProvider tokenProvider) : IAzureResources
    {
        private const int MaxServiceBusConcurrency = 10;
        private const int MaxKeyVaultReferenceRetryAttempts = 3;
        private static readonly TimeSpan MaxKeyVaultReferenceRetryDelay = TimeSpan.FromSeconds(30);

        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly ILogger<AzureResources> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IOAuthTokenProvider _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));

        public async Task<IEnumerable<AzureSubscription>> RetrieveSubscriptionListAsync()
        {
            try
            {
                string url = $"https://management.azure.com/subscriptions?api-version=2025-04-01";
                var subResp = await _httpConnector.GetAsync<AzureSubscriptionsResponse>(url, bearerToken: await _tokenProvider.AcquireTokenAsync("https://management.azure.com"));
                return subResp.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscription list from Azure Management API.");
                return Enumerable.Empty<AzureSubscription>();
            }
        }

        public async Task<IEnumerable<AzureResourceGroup>> RetrieveResourceGroupForSubscriptionAsync(string subscriptionId, string projectCode)
        {
            try
            {
                var groups = new List<AzureResourceGroup>();
                string url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourcegroups?api-version=2025-04-01";
                while (!string.IsNullOrEmpty(url))
                {
                    var rgResponse = await _httpConnector.GetAsync<AzureResourceGroupListResponse>(url, bearerToken: await _tokenProvider.AcquireTokenAsync("https://management.azure.com"));
                    groups.AddRange(rgResponse.Value.Where(z => z.Tags?.ProjectCode == projectCode || projectCode.IsNullOrWhiteSpace()));
                    url = rgResponse.NextLink;  // Handle pagination if there are more resource groups than can be returned in a single response
                }
                return groups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resource groups for subscription {SubscriptionId} from Azure Management API.", subscriptionId);
                return Enumerable.Empty<AzureResourceGroup>();
            }
        }

        public async Task<IEnumerable<AzureResource>> RetrieveAllResourcesForResourceGroupAsync(string subscriptionId, string resourceGroupName)
        {
            try
            {
                var resources = new List<AzureResource>();
                string url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/resources?api-version=2025-04-01";
                while (!string.IsNullOrEmpty(url))
                {
                    var response = await _httpConnector.GetAsync<AzureResourceListResponse>(url, bearerToken: await _tokenProvider.AcquireTokenAsync("https://management.azure.com"));
                    if (response?.Value?.Any() ?? false)
                    {
                        resources.AddRange(response.Value);
                    }
                    // Try to get NextLink for pagination
                    url = response?.NextLink;
                }
                return resources;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all resources for resource group {ResourceGroupName} in subscription {SubscriptionId} from Azure Management API.", resourceGroupName, subscriptionId);
                return Enumerable.Empty<AzureResource>();
            }
        }

        public async Task<IEnumerable<AzureResource>> RetrieveAppServiceSitesForResourceGroupAsync(string subscriptionId, string resourceGroupName)
        {
            var resources = await RetrieveAllResourcesForResourceGroupAsync(subscriptionId, resourceGroupName);
            if (resources?.Any() ?? false)
            {
                return resources.Where(r => string.Equals(r.Type, "Microsoft.Web/sites", StringComparison.OrdinalIgnoreCase));
            }
            return Enumerable.Empty<AzureResource>();
        }

        public async Task<IEnumerable<AzureServiceBusNamespace>> RetrieveServiceBusNamespacesForResourceGroupAsync(string subscriptionId, string resourceGroupName)
        {
            var resources = await RetrieveAllResourcesForResourceGroupAsync(subscriptionId, resourceGroupName);
            if (resources?.Any() ?? false)
            {
                var namespaces = resources
                    .Where(r => string.Equals(r.Type, "Microsoft.ServiceBus/namespaces", StringComparison.OrdinalIgnoreCase))
                    .Select(r => new AzureServiceBusNamespace
                    {
                        Name = r.Name,
                        Type = r.Type,
                        Kind = r.Kind
                    })
                    .ToList();

                using var semaphore = new SemaphoreSlim(MaxServiceBusConcurrency);

                var retrievalTasks = namespaces.Select(async serviceBusNamespace =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        serviceBusNamespace.Topics = await RetrieveServiceBusTopicsAndSubscriptionsForNamespaceAsync(
                            subscriptionId,
                            resourceGroupName,
                            serviceBusNamespace.Name);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(retrievalTasks);

                return namespaces;
            }
            return Enumerable.Empty<AzureServiceBusNamespace>();
        }

        private async Task<IEnumerable<AzureServiceBusTopic>> RetrieveServiceBusTopicsAndSubscriptionsForNamespaceAsync(
            string subscriptionId,
            string resourceGroupName,
            string namespaceName)
        {
            try
            {
                var topics = new List<AzureServiceBusTopic>();
                var namespaceNameSegment = Uri.EscapeDataString(namespaceName);
                string url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.ServiceBus/namespaces/{namespaceNameSegment}/topics?api-version=2024-01-01";

                while (!string.IsNullOrEmpty(url))
                {
                    var response = await _httpConnector.GetAsync<AzureServiceBusTopicListResponse>(url, bearerToken: await _tokenProvider.AcquireTokenAsync("https://management.azure.com"));
                    if (response?.Value?.Any() ?? false)
                    {
                        topics.AddRange(response.Value);
                    }
                    url = response?.NextLink;
                }

                using var semaphore = new SemaphoreSlim(MaxServiceBusConcurrency);

                var retrievalTasks = topics.Select(async topic =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        topic.Subscriptions = await RetrieveServiceBusSubscriptionsForTopicAsync(
                            subscriptionId,
                            resourceGroupName,
                            namespaceName,
                            topic.Name);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(retrievalTasks);

                return topics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Service Bus topics for namespace {NamespaceName} in resource group {ResourceGroupName} and subscription {SubscriptionId} from Azure Management API.", namespaceName, resourceGroupName, subscriptionId);
                return Enumerable.Empty<AzureServiceBusTopic>();
            }
        }

        private async Task<IEnumerable<AzureServiceBusSubscription>> RetrieveServiceBusSubscriptionsForTopicAsync(
            string subscriptionId,
            string resourceGroupName,
            string namespaceName,
            string topicName)
        {
            try
            {
                var subscriptions = new List<AzureServiceBusSubscription>();
                var namespaceNameSegment = Uri.EscapeDataString(namespaceName);
                var topicNameSegment = Uri.EscapeDataString(topicName);
                string url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.ServiceBus/namespaces/{namespaceNameSegment}/topics/{topicNameSegment}/subscriptions?api-version=2024-01-01";

                while (!string.IsNullOrEmpty(url))
                {
                    var response = await _httpConnector.GetAsync<AzureServiceBusSubscriptionListResponse>(url, bearerToken: await _tokenProvider.AcquireTokenAsync("https://management.azure.com"));
                    if (response?.Value?.Any() ?? false)
                    {
                        subscriptions.AddRange(response.Value);
                    }
                    url = response?.NextLink;
                }

                return subscriptions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Service Bus subscriptions for topic {TopicName} in namespace {NamespaceName}, resource group {ResourceGroupName}, and subscription {SubscriptionId} from Azure Management API.", topicName, namespaceName, resourceGroupName, subscriptionId);
                return Enumerable.Empty<AzureServiceBusSubscription>();
            }
        }

        public async Task<IEnumerable<AzureAppSetting>> RetrieveAppSettingsAsync(string subscriptionId, string resourceGroupName, string appName)
        {
            try
            {
                string url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Web/sites/{appName}/config/appsettings/list?api-version=2025-05-01";
                var response = await _httpConnector.PostAsync<AzureAppSettingsResponse>(url, new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json"), bearerToken: await _tokenProvider.AcquireTokenAsync("https://management.azure.com"));
                return response?.Properties?.Select(kv => new AzureAppSetting { Name = kv.Key, Value = kv.Value }) ?? Enumerable.Empty<AzureAppSetting>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving app settings for app {AppName} in resource group {ResourceGroupName} and subscription {SubscriptionId} from Azure Management API.", appName, resourceGroupName, subscriptionId);
                throw;
            }
        }

        public async Task<IEnumerable<AzureKeyVaultReference>> RetrieveKeyVaultResolverStatusAsync(string subscriptionId, string resourceGroupName, string appName)
        {
            try
            {
                var references = new List<AzureKeyVaultReference>();
                var subscriptionIdSegment = Uri.EscapeDataString(subscriptionId);
                var resourceGroupNameSegment = Uri.EscapeDataString(resourceGroupName);
                var appNameSegment = Uri.EscapeDataString(appName);
                string url = $"https://management.azure.com/subscriptions/{subscriptionIdSegment}/resourceGroups/{resourceGroupNameSegment}/providers/Microsoft.Web/sites/{appNameSegment}/config/configreferences/appsettings?api-version=2025-05-01";

                while (!string.IsNullOrEmpty(url))
                {
                    var response = await GetKeyVaultReferencesWithRetryAsync(url);
                    if (response?.Value?.Any() ?? false)
                    {
                        references.AddRange(response.Value);
                    }

                    url = response?.NextLink;
                }

                return references;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Key Vault resolver status for app {AppName} in resource group {ResourceGroupName} and subscription {SubscriptionId} from Azure Management API.", appName, resourceGroupName, subscriptionId);
                throw;
            }
        }

        private async Task<AzureKeyVaultReferencesResponse> GetKeyVaultReferencesWithRetryAsync(string url)
        {
            var bearerToken = await _tokenProvider.AcquireTokenAsync("https://management.azure.com");

            for (var retryAttempt = 0; ; retryAttempt++)
            {
                using var response = await _httpConnector.GetAsync(
                    url,
                    headers: null,
                    bearerToken: bearerToken);

                if (response.StatusCode != HttpStatusCode.TooManyRequests)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw CreateHttpRequestException(response, url, responseContent);
                    }

                    return Json.Deserialize<AzureKeyVaultReferencesResponse>(responseContent);
                }

                if (retryAttempt >= MaxKeyVaultReferenceRetryAttempts)
                {
                    throw new HttpRequestException(
                        $"Azure App Service config reference request remained rate limited after {retryAttempt + 1} attempts.",
                        inner: null,
                        response.StatusCode);
                }

                var retryDelay = GetRetryDelay(response, retryAttempt);
                _logger.LogWarning(
                    "Azure App Service config reference request was rate limited. Retrying after {RetryDelayMs}ms (attempt {NextAttempt} of {MaxAttempts}).",
                    retryDelay.TotalMilliseconds,
                    retryAttempt + 2,
                    MaxKeyVaultReferenceRetryAttempts + 1);

                await Task.Delay(retryDelay);
            }
        }

        private static TimeSpan GetRetryDelay(HttpResponseMessage response, int retryAttempt)
        {
            var retryAfter = response.Headers.RetryAfter;
            var retryAfterDelay = retryAfter?.Delta ?? (retryAfter?.Date - DateTimeOffset.UtcNow);

            TimeSpan delay;
            if (retryAfterDelay is null || retryAfterDelay <= TimeSpan.Zero)
            {
                delay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                delay += TimeSpan.FromMilliseconds(Random.Shared.Next(0, 251));
            }
            else
            {
                delay = retryAfterDelay.Value;
            }

            return delay > MaxKeyVaultReferenceRetryDelay ? MaxKeyVaultReferenceRetryDelay : delay;
        }

        private static HttpRequestException CreateHttpRequestException(
            HttpResponseMessage response,
            string requestUrl,
            string responseContent)
        {
            var exception = new HttpRequestException(
                "Azure App Service config reference request failed.",
                inner: null,
                response.StatusCode);

            exception.Data.Add("RequestUri", response.RequestMessage?.RequestUri?.ToString() ?? requestUrl);
            exception.Data.Add("StatusCode", response.StatusCode.ToString());
            exception.Data.Add("ReasonPhrase", response.ReasonPhrase);
            exception.Data.Add("Response", responseContent);

            return exception;
        }
    }
}
