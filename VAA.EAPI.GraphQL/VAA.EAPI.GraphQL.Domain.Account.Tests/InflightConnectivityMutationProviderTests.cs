using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class InflightConnectivityMutationProviderTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<HttpConnector> _logger;
        private readonly InflightConnectivityMutationProvider _provider;

        public InflightConnectivityMutationProviderTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _provider = new InflightConnectivityMutationProvider(_httpConnector);

            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set("MyAccount:InflightConnectivityMSURL", "https://testresources.virginatlantic.com/dev/inflightconnectivity");
            ConfigReader.Set("MyAccount:InflightConnectivityExpiry", "1");
            ConfigReader.Set("MyAccount:InflightConnectivityLimitedExpiry", "10");
            ConfigReader.Set("MyAccount:InflightConnectivityOcpKey", "Test");
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new InflightConnectivityMutationProvider(null));
        }

        [Fact]
        public async Task CreateInflightConnectivity_ApiCallSuccess()
        {
            var result = await _provider.CreateInflightConnectivity(new AccountEnableIFWifiForDeviceRequest
            {
                DeviceId = "7e3c9a4d1f8b2c6e5a907d3f4b1e6c8a2d9f0b7c5e1a3d8f6c2b4e9a1d7f3c8",
                SandboxId = "123",
                Expiry = DateTime.UtcNow.AddMonths(1)
            });

            Assert.Equal("success", result.Status);
        }
    }
}