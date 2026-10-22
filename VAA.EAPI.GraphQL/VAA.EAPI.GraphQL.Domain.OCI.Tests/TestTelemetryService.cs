using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    internal sealed class TestTelemetryService : ITelemetryService
    {
        public List<TelemetryEvent> Events { get; } = [];

        public Task TrackTelemetry(TelemetryEvent telemetryEvent)
        {
            if (telemetryEvent is not null)
                Events.Add(telemetryEvent);

            return Task.CompletedTask;
        }
    }
}
