
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;

namespace VAA.EAPI.Microservices.Insights.Tests
{
    public sealed class ServiceBusHealthCheck : IClassFixture<HealthCheckContextFixture>, IClassFixture<ConfigurationFixture>, IClassFixture<KeyVaultFixture>
    {
        private readonly HealthCheckContextFixture healthCheckContextFixture;
        private readonly ConfigurationFixture configurationFixture;
        private readonly KeyVaultFixture keyVaultFixture;

        public ServiceBusHealthCheck(HealthCheckContextFixture healthCheckContextFixture, ConfigurationFixture configurationFixture, KeyVaultFixture keyVaultFixture)
        {
            this.healthCheckContextFixture = healthCheckContextFixture;
            this.configurationFixture = configurationFixture;
            this.keyVaultFixture = keyVaultFixture;

            this.configurationFixture.InitialiseValueFromKeyVault(Core.Environment.ServiceBusConnectionString, ConfigReader.Get(Core.Environment.ServiceBusConnectionString), this.keyVaultFixture.Current);
        }

        [Fact(Skip = "Cosmos configuration will not be present during pipeline, test to be refactored to work with TDD dependencies")]
        public async Task HealthyHealthCheck()
        {
            var connectionString = ConfigReader.Get(Core.Environment.ServiceBusConnectionString);

            var serviceBusHealthCheck = new VAA.EAPI.Infrastructure.HealthChecks.ServiceBusHealthCheck(connectionString);

            var result = await serviceBusHealthCheck.CheckHealthAsync(healthCheckContextFixture.Context);

            Assert.True(result.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy, $"Was expecting a healthy result.{Environment.NewLine}{Common.Serialisation.Json.Serialize(result)}");
        }

        [Fact(Skip = "Cosmos configuration will not be present during pipeline, test to be refactored to work with TDD dependencies")]
        public async Task UnhealthyHealthCheck()
        {
            var serviceBusHealthCheck = new VAA.EAPI.Infrastructure.HealthChecks.ServiceBusHealthCheck("dummyconnectionstring");

            var result = await serviceBusHealthCheck.CheckHealthAsync(healthCheckContextFixture.Context);

            Assert.True(result.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy, $"Was expecting an unhealthy result.{Environment.NewLine}{Common.Serialisation.Json.Serialize(result)}");
        }
    }
}