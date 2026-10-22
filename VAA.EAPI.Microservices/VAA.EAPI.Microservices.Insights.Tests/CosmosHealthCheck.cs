
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;

namespace VAA.EAPI.Microservices.Insights.Tests
{
    public sealed class CosmosHealthCheck : IClassFixture<HealthCheckContextFixture>, IClassFixture<ConfigurationFixture>, IClassFixture<KeyVaultFixture>
    {
        private readonly HealthCheckContextFixture healthCheckContextFixture;
        private readonly ConfigurationFixture configurationFixture;
        private readonly KeyVaultFixture keyVaultFixture;

        public CosmosHealthCheck(HealthCheckContextFixture healthCheckContextFixture, ConfigurationFixture configurationFixture, KeyVaultFixture keyVaultFixture)
        {
            this.healthCheckContextFixture = healthCheckContextFixture;
            this.configurationFixture = configurationFixture;
            this.keyVaultFixture = keyVaultFixture;

            this.configurationFixture.InitialiseValueFromKeyVault(Core.Environment.CosmosConnectionString, ConfigReader.Try(Core.Environment.CosmosConnectionString), this.keyVaultFixture.Current);
            this.configurationFixture.InitialiseValueFromKeyVault(Core.Environment.CosmosAccountEndpoint, ConfigReader.Try(Core.Environment.CosmosAccountEndpoint), this.keyVaultFixture.Current);
            this.configurationFixture.InitialiseValueFromKeyVault(Core.Environment.CosmosAuthKeyOrResourceToken, ConfigReader.Try(Core.Environment.CosmosAuthKeyOrResourceToken), this.keyVaultFixture.Current);
            this.configurationFixture.InitialiseValueFromKeyVault(Core.Environment.CosmosDatabaseId, ConfigReader.Get(Core.Environment.CosmosDatabaseId), this.keyVaultFixture.Current);
            this.configurationFixture.InitialiseValueFromKeyVault(Core.Environment.CosmosContainerId, ConfigReader.Get(Core.Environment.CosmosContainerId), this.keyVaultFixture.Current);
        }

        [Fact(Skip = "Cosmos configuration will not be present during pipeline, test to be refactored wo work with TDD dependencies")]
        public async Task HealthyHealthCheck()
        {
            string connectionString = ConfigReader.Try(Core.Environment.CosmosConnectionString);
            string accountEndpoint = ConfigReader.Try(Core.Environment.CosmosAccountEndpoint);
            string authKeyOrResourceToken = ConfigReader.Try(Core.Environment.CosmosAuthKeyOrResourceToken);
            string databaseId = ConfigReader.Get(Core.Environment.CosmosDatabaseId);
            string containerId = ConfigReader.Get(Core.Environment.CosmosContainerId);

            var useConnectionString = !connectionString.IsNullOrWhiteSpace();
            var useEndpoint = !accountEndpoint.IsNullOrWhiteSpace() && Uri.IsWellFormedUriString(accountEndpoint, UriKind.Absolute);
            var useAuthKeyOrResourceToken = !authKeyOrResourceToken.IsNullOrWhiteSpace();

            VAA.EAPI.Infrastructure.HealthChecks.CosmosHealthCheck cosmosHealthCheck = useConnectionString ? new(connectionString, databaseId, containerId) : useEndpoint && !useAuthKeyOrResourceToken ? new(accountEndpoint, databaseId, containerId) : new(accountEndpoint, databaseId, containerId, authKeyOrResourceToken);

            var result = await cosmosHealthCheck.CheckHealthAsync(healthCheckContextFixture.Context);

            Assert.True(result.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy, $"Was expecting a healthy result.{Environment.NewLine}{Common.Serialisation.Json.Serialize(result)}");
        }

        [Fact(Skip = "Cosmos configuration will not be present during pipeline, test to be refactored wo work with TDD dependencies")]
        public async Task UnhealthyHealthCheck()
        {
            string accountEndpoint = "http://tempuri.org:443";
            string databaseId = ConfigReader.Get(Core.Environment.CosmosDatabaseId);
            string containerId = ConfigReader.Get(Core.Environment.CosmosContainerId);

            VAA.EAPI.Infrastructure.HealthChecks.CosmosHealthCheck cosmosHealthCheck = new(accountEndpoint, databaseId, containerId);

            var result = await cosmosHealthCheck.CheckHealthAsync(healthCheckContextFixture.Context);

            Assert.True(result.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy, $"Was expecting an unhealthy result.{Environment.NewLine}{Common.Serialisation.Json.Serialize(result)}");
        }
    }
}