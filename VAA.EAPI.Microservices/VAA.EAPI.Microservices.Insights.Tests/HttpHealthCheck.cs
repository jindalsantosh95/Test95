using VAA.EAPI.Core.Xunit.Fixtures;

namespace VAA.EAPI.Microservices.Insights.Tests
{
    public sealed class HttpHealthCheck : IClassFixture<HealthCheckContextFixture>, IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HealthCheckContextFixture healthCheckContextFixture;
        private readonly HttpClientFactoryFixture httpClientFactoryFixture;

        public HttpHealthCheck(HealthCheckContextFixture healthCheckContextFixture, HttpClientFactoryFixture httpClientFactoryFixture)
        {
            this.healthCheckContextFixture = healthCheckContextFixture;
            this.httpClientFactoryFixture = httpClientFactoryFixture;

            var headers = new Dictionary<string, string>()
            {
                { "User-Agent", "HttpClientFactory/v1.0" },
                { "loadTest", "False" },
                { "api-diagnostics", "False"},
                { "api-channel", "API" },
                { "api-session-id", Guid.Empty.ToString() },
                { "api-correlation-id", Guid.Empty.ToString() },
                { "ocp-apim-trace", "False" }
             };

            var healthyRequest = new HttpRequestMessage(HttpMethod.Get, "http://www.google.co.uk");

            foreach (var header in headers)
                healthyRequest.Headers.Add(header.Key, header.Value);

            var unHealthyRequest = new HttpRequestMessage(HttpMethod.Get, "http://tempuri.co.uk");

            foreach (var header in headers)
                unHealthyRequest.Headers.Add(header.Key, header.Value);

            httpClientFactoryFixture.RegisterRequestResponse(healthyRequest, new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            httpClientFactoryFixture.RegisterRequestResponse(unHealthyRequest, new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
        }

        [Fact]
        public async Task HealthyHealthCheck()
        {
            var apiHealthCheck = new VAA.EAPI.Infrastructure.HealthChecks.HttpHealthCheck(httpClientFactoryFixture.Current, "http://www.google.co.uk", HttpMethod.Get, System.Net.HttpStatusCode.Gone);

            var result = await apiHealthCheck.CheckHealthAsync(healthCheckContextFixture.Context);

            Assert.True(result.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy, $"Was expecting a healthy result.{Environment.NewLine}{Common.Serialisation.Json.Serialize(result)}");
        }

        [Fact]
        public async Task UnhealthyHealthCheck()
        {
            var apiHealthCheck = new VAA.EAPI.Infrastructure.HealthChecks.HttpHealthCheck(httpClientFactoryFixture.Current, "http://tempuri.co.uk", HttpMethod.Get, System.Net.HttpStatusCode.OK);

            var result = await apiHealthCheck.CheckHealthAsync(healthCheckContextFixture.Context);

            Assert.True(result.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy, $"Was expecting an unhealthy result.{Environment.NewLine}{Common.Serialisation.Json.Serialize(result)}");
        }
    }
}