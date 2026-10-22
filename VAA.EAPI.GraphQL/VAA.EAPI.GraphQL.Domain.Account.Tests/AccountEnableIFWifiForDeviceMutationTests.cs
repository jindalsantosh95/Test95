using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;
using XunitHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class AccountEnableIFWifiForDeviceMutationTests
    {
        private const string AnalyticsUrl = "https://tempuri.org/analyticsService/track";
        private readonly IServiceProvider _serviceProvider;
        private readonly XunitHttpClientFactory _httpClientFactory;

        public AccountEnableIFWifiForDeviceMutationTests()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set("MyAccount:InflightConnectivityMSURL", "https://testresources.virginatlantic.com/dev/inflightconnectivity");
            ConfigReader.Set("MyAccount:InflightConnectivityExpiry", "1");
            ConfigReader.Set("MyAccount:InflightConnectivityLimitedExpiry", "10");
            ConfigReader.Set("MyAccount:InflightConnectivityOcpKey", "Test");
            ConfigReader.Set(Environment.AnalyticsMSUrl, AnalyticsUrl);
            ConfigReader.Set(Environment.AanalyticsOcpSubId, "Test");

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory, XunitHttpClientFactory>()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider_AAD>()
                .AddSingleton<IAccountCustomerService, CustomerService>()
                .AddSingleton<ITokenB2CProvider<string>, TokenB2CProvider<string>>()
                .AddSingleton<IInflightConnectivityMutationProvider, InflightConnectivityMutationProvider>()
                .AddSingleton<IAnalyticsService, AnalyticsService>()
                .AddSingleton<AccountEnableIFWifiForDeviceMutation>()
                .BuildServiceProvider();

            _httpClientFactory = (XunitHttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>();
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenAnalyticsServiceObjectIsNull()
        {
            IAnalyticsService analyticsService = null!;

            Assert.Throws<ArgumentNullException>(() => new AccountEnableIFWifiForDeviceMutation(
                _serviceProvider.GetRequiredService<IInflightConnectivityMutationProvider>(),
                _serviceProvider.GetRequiredService<ITokenB2CProvider<string>>(),
                _serviceProvider.GetRequiredService<ILogger<AccountEnableIFWifiForDeviceMutation>>(),
                analyticsService));
        }

        [Theory]
        [InlineData(DeviceInflightEnableType.Under18)]
        [InlineData(DeviceInflightEnableType.Enrol)]
        [InlineData(DeviceInflightEnableType.Crew)]
        public async Task SetDeviceInflightState_ShouldTrackEnableTypeWithoutPii(DeviceInflightEnableType enableType)
        {
            var deviceId = $"pii-device-id-{Guid.NewGuid():N}";
            var sandboxId = $"pii-sandbox-id-{Guid.NewGuid():N}";
            var analyticsCount = CountAnalyticsResponses();
            var mutation = _serviceProvider.GetRequiredService<AccountEnableIFWifiForDeviceMutation>();

            //Act
            var result = await mutation.SetDeviceInflightState(deviceId, sandboxId, enableType);

            //Assert
            Assert.Equal("success", result.Status);

            // Wait briefly for the fire-and-forget analytics task to complete (avoid test flakiness)
            for (var i = 0; i < 20 && GetAnalyticsResponses().Count < analyticsCount + 1; i++)
            {
                await Task.Delay(50);
            }

            IReadOnlyList<HttpResponseMessage> analyticsResponses;
            var deadline = DateTime.UtcNow.AddSeconds(2);

            do
            {
                analyticsResponses = GetAnalyticsResponses();
                if (analyticsResponses.Count >= analyticsCount + 1)
                {
                    break;
                }

                await Task.Delay(25);
            }
            while (DateTime.UtcNow < deadline);

            Assert.Equal(analyticsCount + 1, analyticsResponses.Count);
            var analyticsRequest = analyticsResponses.Last().RequestMessage;
            Assert.NotNull(analyticsRequest);

            var analyticsContent = await analyticsRequest!.Content!.ReadAsStringAsync();
            var analyticsPayload = JObject.Parse(analyticsContent);
            Assert.Equal("GraphQL", analyticsPayload["type"]?.ToString());
            Assert.Equal("SetDeviceInflightState", analyticsPayload["subject"]?.ToString());
            Assert.Equal(enableType.ToString(), analyticsPayload.SelectToken("data.EnableType")?.ToString());
            Assert.Null(analyticsPayload.SelectToken("data.DeviceId"));
            Assert.Null(analyticsPayload.SelectToken("data.SandboxId"));
            Assert.DoesNotContain(deviceId, analyticsContent);
            Assert.DoesNotContain(sandboxId, analyticsContent);
        }

        [Fact]
        public async Task SetDeviceInflightState_ShouldNotTrackAnalytics_WhenEnableTypeIsNotProvided()
        {
            var analyticsCount = CountAnalyticsResponses();
            var mutation = _serviceProvider.GetRequiredService<AccountEnableIFWifiForDeviceMutation>();

            //Act
            var result = await mutation.SetDeviceInflightState(
                $"pii-device-id-{Guid.NewGuid():N}",
                $"pii-sandbox-id-{Guid.NewGuid():N}");

            //Assert
            Assert.Equal("success", result.Status);
            Assert.Equal(analyticsCount, CountAnalyticsResponses());
        }

        private int CountAnalyticsResponses()
        {
            return GetAnalyticsResponses().Count;
        }

        private IReadOnlyList<HttpResponseMessage> GetAnalyticsResponses()
        {
            return _httpClientFactory.GetResponses(AnalyticsUrl)
                .Where(response => AnalyticsUrl.Equals(response.RequestMessage?.RequestUri?.ToString(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}