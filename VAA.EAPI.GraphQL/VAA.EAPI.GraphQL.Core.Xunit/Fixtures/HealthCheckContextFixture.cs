using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace VAA.EAPI.GraphQL.Core.Xunit.Fixtures
{
    public sealed class HealthCheckContextFixture : IDisposable
    {
        public HealthCheckContext Context { get; }

        public HealthCheckContextFixture()
        {
            Context = new HealthCheckContext()
            {
                Registration = new HealthCheckRegistration("Test Fixture", new HealthCheck(), HealthStatus.Unhealthy, new string[] { "unit test" })
            };
        }

        public void Dispose()
        {
        }

        private sealed class HealthCheck : IHealthCheck
        {
            public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }
    }
}