
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;

namespace VAA.EAPI.Microservices.Insights.Tests
{
    public sealed class SqlHealthCheck : IClassFixture<HealthCheckContextFixture>, IClassFixture<ConfigurationFixture>
    {
        private readonly HealthCheckContextFixture healthCheckContextFixture;
        private readonly ConfigurationFixture configurationFixture;

        public SqlHealthCheck(HealthCheckContextFixture healthCheckContextFixture, ConfigurationFixture configurationFixture)
        {
            this.healthCheckContextFixture = healthCheckContextFixture;
            this.configurationFixture = configurationFixture;

            AppDomain.CurrentDomain.SetData("DataDirectory", System.Environment.CurrentDirectory);

            this.configurationFixture.InitialiseValue(Core.Environment.SqlConnectionString, "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=SSPI;Connect Timeout=60;AttachDbFilename=\"|DataDirectory|\\SqlHealthCheck.mdf\"".Replace("|DataDirectory|", System.Environment.CurrentDirectory));
        }

        [Fact]
        public async Task HealthyHealthCheck()
        {
            var connectionString = ConfigReader.Get(Core.Environment.SqlConnectionString);

            var sqlHealthCheck = new VAA.EAPI.Infrastructure.HealthChecks.SqlHealthCheck(connectionString);

            var result = await sqlHealthCheck.CheckHealthAsync(healthCheckContextFixture.Context);

            Assert.True(result.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy, $"Was expecting a healthy result.{Environment.NewLine}{Common.Serialisation.Json.Serialize(result)}");
        }

        [Fact]
        public async Task UnhealthyHealthCheck()
        {
            var sqlHealthCheck = new VAA.EAPI.Infrastructure.HealthChecks.SqlHealthCheck("dummyconnectionstring");

            var result = await sqlHealthCheck.CheckHealthAsync(healthCheckContextFixture.Context);

            Assert.True(result.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy, $"Was expecting an unhealthy result.{Environment.NewLine}{Common.Serialisation.Json.Serialize(result)}");
        }
    }
}