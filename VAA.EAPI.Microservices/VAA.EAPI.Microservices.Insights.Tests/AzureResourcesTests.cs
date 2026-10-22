using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Insights.Infrastructure.Azure;

namespace VAA.EAPI.Microservices.Insights.Tests
{
    public class AzureResourcesTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly VAA.EAPI.Core.Xunit.Http.HttpClientFactory _httpClientFactory;

        public AzureResourcesTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _httpClientFactory = httpClientFactoryFixture.Current;
            _serviceProvider ??= new ServiceCollection().AddLogging()
                                                        .AddSingleton<IHttpClientFactory>(_httpClientFactory)
                                                        .AddSingleton<IHttpConnector, HttpConnector>()
                                                        .AddScoped<VAA.EAPI.Microservices.Insights.Infrastructure.Azure.IAzureResources, VAA.EAPI.Microservices.Insights.Infrastructure.Azure.AzureResources>()
                                                        .AddScoped<IOAuthTokenProvider, VAA.EAPI.Core.Xunit.Azure.OAuthTokenProvider>()
                                                        .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new VAA.EAPI.Core.Functions.TestFunctionContext() };
            ConfigReader.Set(Common.Constants.Environment.AADAuthority, "aad-auth");
            ConfigReader.Set(Common.Constants.Environment.AADTenant, "aad-tenant");
            ConfigReader.Set(Common.Constants.Environment.EapiMSSecret, "eapi-ms-secret");

        }

        [Fact]
        public async Task RetrieveKeyVaultResolverStatusAsync_RetriesThenThrows_WhenRateLimitPersists()
        {
            // Arrange
            ConfigReader.Set(Common.Constants.Environment.EapiMsClientID, "GOOD_TOKEN");
            var azureResources = _serviceProvider.GetRequiredService<IAzureResources>();
            const string requestUrl = "https://management.azure.com/subscriptions/test-subscription-id/resourceGroups/test-resource-group/providers/Microsoft.Web/sites/test-rate-limited-app/config/configreferences/appsettings?api-version=2025-05-01";
            var responseCountBefore = _httpClientFactory.GetResponses(requestUrl).Count(response => response.RequestMessage?.RequestUri?.ToString() == requestUrl);

            // Act
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
                azureResources.RetrieveKeyVaultResolverStatusAsync("test-subscription-id", "test-resource-group", "test-rate-limited-app"));

            // Assert
            var responseCountAfter = _httpClientFactory.GetResponses(requestUrl).Count(response => response.RequestMessage?.RequestUri?.ToString() == requestUrl);
            Assert.Equal(System.Net.HttpStatusCode.TooManyRequests, exception.StatusCode);
            Assert.Equal(4, responseCountAfter - responseCountBefore);
        }

        [Fact]
        public async Task RetrieveKeyVaultResolverStatusAsync_PreservesResponseDetails_WithoutRetryingNonRateLimitFailure()
        {
            // Arrange
            ConfigReader.Set(Common.Constants.Environment.EapiMsClientID, "GOOD_TOKEN");
            var azureResources = _serviceProvider.GetRequiredService<IAzureResources>();
            const string requestUrl = "https://management.azure.com/subscriptions/test-subscription-id/resourceGroups/test-resource-group/providers/Microsoft.Web/sites/test-unavailable-app/config/configreferences/appsettings?api-version=2025-05-01";
            var responseCountBefore = _httpClientFactory.GetResponses(requestUrl).Count(response => response.RequestMessage?.RequestUri?.ToString() == requestUrl);

            // Act
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
                azureResources.RetrieveKeyVaultResolverStatusAsync("test-subscription-id", "test-resource-group", "test-unavailable-app"));

            // Assert
            var responseCountAfter = _httpClientFactory.GetResponses(requestUrl).Count(response => response.RequestMessage?.RequestUri?.ToString() == requestUrl);
            Assert.Equal(System.Net.HttpStatusCode.ServiceUnavailable, exception.StatusCode);
            Assert.Equal(requestUrl, exception.Data["RequestUri"]);
            Assert.Equal("ServiceUnavailable", exception.Data["StatusCode"]);
            Assert.Contains("Service temporarily unavailable", exception.Data["Response"]?.ToString());
            Assert.Equal(1, responseCountAfter - responseCountBefore);
        }

        [Theory]
        [InlineData("GOOD_TOKEN")]
        //[InlineData("BAD_TOKEN")]
        //[InlineData("ERROR_TOKEN")]
        public async Task RetrieveSubscriptionListAsync_ShouldReturnSubscriptions(string token)
        {
            // Arrange
            ConfigReader.Set(Common.Constants.Environment.EapiMsClientID, token);


            var azureResources = _serviceProvider.GetRequiredService<IAzureResources>();
            // Act
            var subscriptions = await azureResources.RetrieveSubscriptionListAsync();

            // Assertions
            Assert.NotNull(subscriptions);
            if (token == "GOOD_TOKEN")
            {
                Assert.Equal(3, subscriptions.Count()); // There are 3 entries in the mock file
            }
            else
            {
                Assert.Empty(subscriptions); // There are 0 entries in the mock file
            }
        }

        [Theory]
        [InlineData("GOOD_TOKEN")]
        //[InlineData("BAD_TOKEN")]
        //[InlineData("ERROR_TOKEN")]
        public async Task RetrieveResourceGroupForSubscriptionAsync_ShouldReturnResourceGroups(string token)
        {
            // Arrange
            ConfigReader.Set(Common.Constants.Environment.EapiMsClientID, token);

            var azureResources = _serviceProvider.GetRequiredService<IAzureResources>();
            string subscriptionId = "test-subscription-id"; // Replace with a valid subscriptionId for integration
            string projectCode = string.Empty; // Or set a test project code

            // Act
            var resourceGroups = await azureResources.RetrieveResourceGroupForSubscriptionAsync(subscriptionId, projectCode);

            // Assert
            Assert.NotNull(resourceGroups);
            if (token == "GOOD_TOKEN")
            {
                Assert.Equal(2, resourceGroups.Count()); // There are 2 entries in the mock file
            }
            else
            {
                Assert.Empty(resourceGroups); // There are 0 entries in the mock file

            }
        }

        [Theory]
        [InlineData("GOOD_TOKEN")]
        //[InlineData("BAD_TOKEN")]
        //[InlineData("ERROR_TOKEN")]
        public async Task RetrieveAppServiceSitesForResourceGroupAsync_ShouldReturnSites(string token)
        {
            // Arrange
            ConfigReader.Set(Common.Constants.Environment.EapiMsClientID, token);

            var azureResources = _serviceProvider.GetRequiredService<IAzureResources>();
            string subscriptionId = "test-subscription-id"; // Replace with a valid subscriptionId for integration
            string resourceGroupName = "test-resource-group"; // Replace with a valid resource group name

            // Act
            var sites = await azureResources.RetrieveAppServiceSitesForResourceGroupAsync(subscriptionId, resourceGroupName);

            // Assert
            Assert.NotNull(sites);
            if (token == "GOOD_TOKEN")
            {
                Assert.Equal(56, sites.Count()); // There are 56 entries in the mock file
            }
            else
            {
                Assert.Empty(sites); // There are 0 entries in the mock file
            }
        }

        [Theory]
        [InlineData("GOOD_TOKEN")]
        //[InlineData("BAD_TOKEN")]
        //[InlineData("ERROR_TOKEN")]
        public async Task RetrieveServiceBusNamespacesForResourceGroupAsync_ShouldReturnNamespaces(string token)
        {
            // Arrange
            ConfigReader.Set(Common.Constants.Environment.EapiMsClientID, token);

            var azureResources = _serviceProvider.GetRequiredService<IAzureResources>();
            string subscriptionId = "test-subscription-id"; // Replace with a valid subscriptionId for integration
            string resourceGroupName = "test-servicebus-resource-group"; // Replace with a valid resource group name

            // Act
            var namespaces = await azureResources.RetrieveServiceBusNamespacesForResourceGroupAsync(subscriptionId, resourceGroupName);

            // Assert
            Assert.NotNull(namespaces);
            if (token == "GOOD_TOKEN")
            {
                var serviceBusNamespace = Assert.Single(namespaces);
                Assert.Equal("test-servicebus-namespace", serviceBusNamespace.Name);
                Assert.Equal("Microsoft.ServiceBus/namespaces", serviceBusNamespace.Type);

                var topic = Assert.Single(serviceBusNamespace.Topics);
                Assert.Equal("test-topic", topic.Name);

                var topicSubscription = Assert.Single(topic.Subscriptions);
                Assert.Equal("test-subscription", topicSubscription.Name);
                Assert.Equal(7, topicSubscription.ActiveQueueDepth);
                Assert.Equal(2, topicSubscription.DeadLetterQueueDepth);
            }
            else
            {
                Assert.Empty(namespaces);
            }
        }

        [Theory]
        [InlineData("GOOD_TOKEN")]
        //[InlineData("BAD_TOKEN")]
        //[InlineData("ERROR_TOKEN")]
        public async Task RetrieveAllResourcesForResourceGroupAsync_ShouldReturnList(string token)
        {
            // Arrange
            ConfigReader.Set(Common.Constants.Environment.EapiMsClientID, token);

            var azureResources = _serviceProvider.GetRequiredService<IAzureResources>();
            string subscriptionId = "test-subscription-id"; // Replace with a valid subscriptionId for integration
            string resourceGroupName = "test-resource-group"; // Replace with a valid resource group name

            // Act
            var resources = await azureResources.RetrieveAllResourcesForResourceGroupAsync(subscriptionId, resourceGroupName);
            // Assert
            Assert.NotNull(resources);
            if (token == "GOOD_TOKEN")
            {
                Assert.Equal(477, resources.Count()); // There are 477 entries in the mock file
            }
            else
            {
                Assert.Empty(resources); // There are 0 entries in the mock file
            }
        }

        [Theory]
        [InlineData("GOOD_TOKEN")]
        //[InlineData("BAD_TOKEN")]
        //[InlineData("ERROR_TOKEN")]
        public async Task RetrieveAppSettingsAsync_ShouldReturnAppSettings(string token)
        {
            // Arrange
            ConfigReader.Set(Common.Constants.Environment.EapiMsClientID, token);

            var azureResources = _serviceProvider.GetRequiredService<IAzureResources>();
            string subscriptionId = "test-subscription-id"; // Replace with a valid subscriptionId for integration
            string resourceGroupName = "test-resource-group"; // Replace with a valid resource group name
            string appName = "test-app-name"; // Replace with a valid app name

            // Act
            var appSettings = await azureResources.RetrieveAppSettingsAsync(subscriptionId, resourceGroupName, appName);
            // Assert
            Assert.NotNull(appSettings);
            if (token == "GOOD_TOKEN")
            {
                Assert.Equal(4, appSettings.Count()); // There are 4 entries in the mock file
            }
            else
            {
                Assert.Empty(appSettings); // There are 0 entries in the mock file
            }
        }

        [Theory]
        [InlineData("test-app-name", 2)]
        [InlineData("test-empty-app-name", 0)]
        public async Task RetrieveKeyVaultResolverStatusAsync_ShouldReturnKeyVaultReferences(string appName, int expectedCount)
        {
            // Arrange
            ConfigReader.Set(Common.Constants.Environment.EapiMsClientID, "GOOD_TOKEN");

            var azureResources = _serviceProvider.GetRequiredService<IAzureResources>();
            string subscriptionId = "test-subscription-id";
            string resourceGroupName = "test-resource-group";

            // Act
            var references = (await azureResources.RetrieveKeyVaultResolverStatusAsync(subscriptionId, resourceGroupName, appName)).ToList();

            // Assert
            Assert.Equal(expectedCount, references.Count);

            if (expectedCount > 0)
            {
                Assert.Equal("resolved-setting", references[0].Name);
                Assert.Equal("Resolved", references[0].Properties.Status);
                Assert.Equal("denied-setting", references[1].Name);
                Assert.Equal("AccessToKeyVaultDenied", references[1].Properties.Status);
            }
        }


    }
}
