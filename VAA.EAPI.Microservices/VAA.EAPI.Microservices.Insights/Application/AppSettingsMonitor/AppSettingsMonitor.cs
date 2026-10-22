using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Insights.Infrastructure.Azure;
using VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models;
using VAA.EAPI.Microservices.Insights.Models.AppSettingsMonitor;

namespace VAA.EAPI.Microservices.Insights.Application.AppSettingsMonitor
{
    public sealed class AppSettingsMonitor(IAzureResources azureResources, IHttpConnector httpConnector, ILogger<AppSettingsMonitor> logger, IOAuthTokenProvider tokenProvider) : IAppSettingsMonitor
    {
        private readonly ILogger<AppSettingsMonitor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IAzureResources _azureResources = azureResources ?? throw new ArgumentNullException(nameof(azureResources));
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly IOAuthTokenProvider _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        private static BlobServiceClient _blobServiceClient;
        private static BlobContainerClient _containerClient;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Extracts recent "[diff]" alerts from Application Insights by querying trace logs for messages containing the "[diff]" tag within a specified timeframe. Parses the relevant details (subscription, resource group, function app) from the log messages and returns a list of AppInsightAlert objects representing each alert found.
        /// </summary>
        /// <param name="timeframe"></param>
        /// <returns></returns>
        public async Task<IEnumerable<AppInsightAlert>> FindRecentDiffAlerts(TimeSpan timeframe)
        {
            var RecentTracesQuery = $"traces | where message has (\"[diff]\") | where timestamp > ago({timeframe.TotalMinutes}m) | project timestamp, message | order by timestamp desc";

            var appId = ConfigReader.Try(Constants.Environment.AppSettingsMonitorAppInsightsAppId, string.Empty);

            var url = $"https://api.applicationinsights.io/v1/apps/{appId}/query?query={HttpUtility.UrlEncode(RecentTracesQuery)}";
            var response = await _httpConnector.GetAsync<JObject>(url, null, await _tokenProvider.AcquireTokenAsync("https://api.applicationinsights.io"));

            if (response != null)
            {
                var alerts = response["tables"]?[0]?["rows"]?.Select(row =>
                {
                    Match m = System.Text.RegularExpressions.Regex.Match(row[1]?.Value<string>() ?? string.Empty, @"Subscription:\s*(.+?),\s*ResourceGroup:\s*(.+?),\s*FunctionApp:\s*(.+)$");
                    return new AppInsightAlert
                    {
                        Timestamp = row[0]?.Value<DateTime>() ?? DateTime.MinValue,
                        Subscription = m.Groups[1].Value,
                        ResourceGroup = m.Groups[2].Value,
                        AppName = m.Groups[3].Value
                    };
                }).ToList() ?? new List<AppInsightAlert>();
                return alerts;
            }

            return Enumerable.Empty<AppInsightAlert>();
        }


        /// <summary>
        /// Iterates through all subscriptions, resource groups, and function apps, checking for differences in app settings.
        /// Logs details when differences are found.
        /// </summary>
        public async Task FullCompare()
        {
            // If there are subscription IDs specified in the configuration, filter the subscriptions to only include those; otherwise, include all subscriptions
            var subscriptions = await _azureResources.RetrieveSubscriptionListAsync();

            // Iterate through each subscription, resource group, and function app to check for differences in app settings
            foreach (var subscription in subscriptions)
            {
                // We only want Digital resource groups for now (project code=345)
                var resourceGroups = await _azureResources.RetrieveResourceGroupForSubscriptionAsync(subscription.SubscriptionId, "345");
                foreach (var rg in resourceGroups)
                {
                    var rgResources = await _azureResources.RetrieveAppServiceSitesForResourceGroupAsync(subscription.SubscriptionId, rg.Name);
                    var functionApps = rgResources.Where(site => site.Kind != null && site.Kind.Contains("functionapp", StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<AzureResource>();

                    foreach (var app in functionApps)
                    {
                        var appSettingsTask = _azureResources.RetrieveAppSettingsAsync(subscription.SubscriptionId, rg.Name, app.Name);
                        var referencesTask = _azureResources.RetrieveKeyVaultResolverStatusAsync(subscription.SubscriptionId, rg.Name, app.Name);
                        List<AzureAppSetting> currentSettings = null;

                        try
                        {
                            currentSettings = (await appSettingsTask).ToList();
                            var differences = (await Compare(subscription.SubscriptionId, rg.Name, app.Name, currentSettings)).AppSettingsState.ToList();
                            if (differences.Any(d => d.CurrentValue != d.PreviousValue))
                            {
                                _logger.LogWarning("[DIFF] Subscription: {SubscriptionName} ({SubscriptionId}), ResourceGroup: {ResourceGroupName}, FunctionApp: {FunctionAppName}", subscription.DisplayName, subscription.SubscriptionId, rg.Name, app.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "App settings comparison failed for Subscription: {SubscriptionName} ({SubscriptionId}), ResourceGroup: {ResourceGroupName}, App: {FunctionAppName}", subscription.DisplayName, subscription.SubscriptionId, rg.Name, app.Name);
                        }

                        try
                        {
                            var references = await referencesTask;
                            if (currentSettings != null)
                            {
                                LogUnresolvedKeyVaultReferences(subscription, rg, app, currentSettings, references);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Key Vault resolver status check failed for Subscription: {SubscriptionName} ({SubscriptionId}), ResourceGroup: {ResourceGroupName}, App: {FunctionAppName}", subscription.DisplayName, subscription.SubscriptionId, rg.Name, app.Name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Iterates through all function apps and logs a warning for every Key Vault app-setting reference
        /// whose resolver status is not healthy.
        /// </summary>
        public async Task FullCheckKeyVaultResolverStatus()
        {
            var subscriptions = await _azureResources.RetrieveSubscriptionListAsync();

            foreach (var subscription in subscriptions)
            {
                var resourceGroups = await _azureResources.RetrieveResourceGroupForSubscriptionAsync(subscription.SubscriptionId, "345");
                foreach (var resourceGroup in resourceGroups)
                {
                    var resources = await _azureResources.RetrieveAppServiceSitesForResourceGroupAsync(subscription.SubscriptionId, resourceGroup.Name);
                    var functionApps = resources.Where(site => site.Kind != null && site.Kind.Contains("functionapp", StringComparison.OrdinalIgnoreCase));

                    foreach (var app in functionApps)
                    {
                        try
                        {
                            var appSettingsTask = _azureResources.RetrieveAppSettingsAsync(subscription.SubscriptionId, resourceGroup.Name, app.Name);
                            var referencesTask = _azureResources.RetrieveKeyVaultResolverStatusAsync(subscription.SubscriptionId, resourceGroup.Name, app.Name);

                            await Task.WhenAll(appSettingsTask, referencesTask);

                            LogUnresolvedKeyVaultReferences(subscription, resourceGroup, app, await appSettingsTask, await referencesTask);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Key Vault resolver status check failed for Subscription: {SubscriptionName} ({SubscriptionId}), ResourceGroup: {ResourceGroupName}, App: {FunctionAppName}",
                                subscription.DisplayName,
                                subscription.SubscriptionId,
                                resourceGroup.Name,
                                app.Name);
                        }
                    }
                }
            }
        }

        public async Task<AppSettingCompareResponse> Snapshot(string subscriptionId, string resourceGroupName, string functionAppName)
        {
            // Get a list of the current settings for the specified Azure Function App
            var currentSettings = await _azureResources.RetrieveAppSettingsAsync(subscriptionId, resourceGroupName, functionAppName);

            // Save the settings
            await SaveSnapshotAsync(subscriptionId, resourceGroupName, functionAppName, currentSettings);

            // Return the current settings as the Snapshot result
            return await Compare(subscriptionId, resourceGroupName, functionAppName);
        }


        public async Task<AppSettingCompareResponse> Compare(string subscriptionId, string resourceGroupName, string functionAppName)
        {
            // Get a list of the current settings for the specified Azure Function App
            var currentSettings = await _azureResources.RetrieveAppSettingsAsync(subscriptionId, resourceGroupName, functionAppName);

            return await Compare(subscriptionId, resourceGroupName, functionAppName, currentSettings);
        }

        private async Task<AppSettingCompareResponse> Compare(string subscriptionId, string resourceGroupName, string functionAppName, IEnumerable<AzureAppSetting> currentSettings)
        {

            // Get a list of the previous settings for the specified Azure Function App from storage (e.g., database, blob storage, etc.)
            var previousSettings = await ReadSnapshotAsync(subscriptionId, resourceGroupName, functionAppName);

            // Combine keys from both current and previous settings
            var allKeys = currentSettings.Select(s => s.Name)
                .Union(previousSettings.AppSettings.Select(s => s.Name))
                .Distinct();

            return new()
            {
                SnapshotDateTime = previousSettings.SnapshotDateTime,
                AppSettingsState = allKeys.Select(k => new AppSettingsState
                {
                    Name = k,
                    CurrentValue = currentSettings.FirstOrDefault(s => s.Name == k)?.Value ?? "<not present>",
                    PreviousValue = previousSettings.AppSettings.FirstOrDefault(s => s.Name == k)?.Value ?? "<not present>"
                }).ToList()
            };
        }

        private void LogUnresolvedKeyVaultReferences(
            AzureSubscription subscription,
            AzureResourceGroup resourceGroup,
            AzureResource app,
            IEnumerable<AzureAppSetting> appSettings,
            IEnumerable<AzureKeyVaultReference> references)
        {
            var referencesBySettingName = references
                .Where(reference => reference?.Name != null)
                .GroupBy(reference => reference.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            var keyVaultSettings = appSettings
                .Where(setting => setting?.Value?.StartsWith("@Microsoft.KeyVault(", StringComparison.OrdinalIgnoreCase) ?? false);

            foreach (var setting in keyVaultSettings)
            {
                referencesBySettingName.TryGetValue(setting.Name, out var reference);
                var resolverStatus = reference?.Properties?.Status;
                if (resolverStatus.Is("Resolved"))
                {
                    continue;
                }

                _logger.LogWarning(
                    "[KEYVAULT-REFERENCE-UNRESOLVED] Subscription: {SubscriptionName} ({SubscriptionId}), ResourceGroup: {ResourceGroupName}, FunctionApp: {FunctionAppName}, AppSetting: {AppSettingName}, ResolverStatus: {ResolverStatus}",
                    subscription.DisplayName,
                    subscription.SubscriptionId,
                    resourceGroup.Name,
                    app.Name,
                    setting.Name,
                    resolverStatus ?? "Unknown");
            }
        }

        private async Task<AppSettingSnapshot> ReadSnapshotAsync(string subscriptionId, string resourceGroupName, string functionAppName)
        {
            var result = new AppSettingSnapshot
            {
                SnapshotDateTime = DateTime.MinValue,
                AppSettings = Enumerable.Empty<AzureAppSetting>().ToList()
            };

            var containerClient = await GetSnapshotContainerAsync();
            var blobClient = containerClient.GetBlobClient(GetSnapshotBlobName(subscriptionId, resourceGroupName, functionAppName));

            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadContentAsync();
                var json = response.Value.Content.ToString();
                result.SnapshotDateTime = response.Value.Details.LastModified.UtcDateTime;
                result.AppSettings = JsonConvert.DeserializeObject<IEnumerable<AzureAppSetting>>(json)?.ToList() ?? [];
            }

            return result;
        }

        private async Task SaveSnapshotAsync(string subscriptionId, string resourceGroupName, string functionAppName, IEnumerable<AzureAppSetting> settings)
        {
            var containerClient = await GetSnapshotContainerAsync();
            var blobClient = containerClient.GetBlobClient(GetSnapshotBlobName(subscriptionId, resourceGroupName, functionAppName));
            var settingsJson = JsonConvert.SerializeObject(settings, Formatting.Indented);

            await blobClient.UploadAsync(BinaryData.FromString(settingsJson), overwrite: true);
        }

        private static string GetSnapshotBlobName(string subscriptionId, string resourceGroupName, string functionAppName) =>
            $"{subscriptionId}/{resourceGroupName}/{functionAppName}.json".ToLowerInvariant();

        private static async Task<BlobContainerClient> GetSnapshotContainerAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_blobServiceClient == null)
                {
                    var storageConnectionString = ConfigReader.Try(Constants.Environment.AppSettingsMonitorSnapshotStorageConnectionString, string.Empty);
                    _blobServiceClient = new BlobServiceClient(storageConnectionString);
                    _containerClient = null; // Reset container client to ensure it will be re-initialized with the new BlobServiceClient
                }
                if (_containerClient == null)
                {
                    var containerName = ConfigReader.Try(Constants.Environment.AppSettingsMonitorSnapshotStorageContainerName, string.Empty);
                    _containerClient = _blobServiceClient.GetBlobContainerClient(containerName.ToLowerInvariant());
                    await _containerClient.CreateIfNotExistsAsync();
                }
                return _containerClient;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
