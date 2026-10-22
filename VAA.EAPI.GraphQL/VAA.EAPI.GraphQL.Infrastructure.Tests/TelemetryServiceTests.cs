using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Diagnostics.Telemetry;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;
using Xunit;

namespace VAA.EAPI.GraphQL.Infrastructure.Tests
{
    public sealed class TelemetryServiceTests
    {
        [Theory]
        [InlineData("AppInsights", 1, 0)]
        [InlineData("Datadog", 0, 1)]
        [InlineData("Both", 1, 1)]
        [InlineData("unsupported", 1, 0)]
        public async Task TrackTelemetry_RoutesToTheConfiguredDestinations(string loggingProvider, int expectedApplicationInsightsEvents, int expectedDatadogEvents)
        {
            ConfigReader.Set("LoggingProvider", loggingProvider);
            var telemetryChannel = new CapturingTelemetryChannel();
            var tracker = new CapturingCustomEventTracker();
            var service = new TelemetryService(CreateTelemetryClient(telemetryChannel), NullLogger<TelemetryService>.Instance, tracker);
            var telemetryEvent = new TelemetryEvent(TestEvent.CheckInCompleted, new Dictionary<string, object>
            {
                ["PnrLocator"] = "not-for-datadog",
                ["PassengerId"] = "not-for-datadog"
            })
            {
                Id = "event-id",
                Source = "GraphQL.Tests",
                DateTimeUtc = "2026-09-17T12:00:00.0000000Z"
            };

            await service.TrackTelemetry(telemetryEvent);

            Assert.Equal(expectedApplicationInsightsEvents, telemetryChannel.Events.Count);
            Assert.Equal(expectedDatadogEvents, tracker.Events.Count);

            if (expectedApplicationInsightsEvents == 1)
            {
                var applicationInsightsEvent = Assert.Single(telemetryChannel.Events);
                Assert.Equal(TestEvent.CheckInCompleted.ToString(), applicationInsightsEvent.Name);
                Assert.Equal("not-for-datadog", applicationInsightsEvent.Properties["PnrLocator"]);
                Assert.Equal("not-for-datadog", applicationInsightsEvent.Properties["PassengerId"]);
            }

            if (expectedDatadogEvents == 1)
            {
                var datadogEvent = Assert.Single(tracker.Events);
                Assert.Equal(TestEvent.CheckInCompleted.ToString(), datadogEvent.Name);
                Assert.Equal("event-id", datadogEvent.Properties["Id"]);
                Assert.Equal("GraphQL.Tests", datadogEvent.Properties["Source"]);
                Assert.Equal("2026-09-17T12:00:00.0000000Z", datadogEvent.Properties["TimestampUtc"]);
                Assert.Equal("Information", datadogEvent.Properties["Severity"]);
                Assert.DoesNotContain("PnrLocator", datadogEvent.Properties.Keys);
                Assert.DoesNotContain("PassengerId", datadogEvent.Properties.Keys);
            }
        }

        [Fact]
        public async Task TrackTelemetry_ContainsDatadogTrackerFailure()
        {
            ConfigReader.Set("LoggingProvider", "Datadog");
            var telemetryChannel = new CapturingTelemetryChannel();
            var service = new TelemetryService(CreateTelemetryClient(telemetryChannel), NullLogger<TelemetryService>.Instance, new ThrowingCustomEventTracker());

            await service.TrackTelemetry(new TelemetryEvent(TestEvent.CheckInCompleted));

            Assert.Empty(telemetryChannel.Events);
        }

        private static TelemetryClient CreateTelemetryClient(CapturingTelemetryChannel telemetryChannel) => new(new TelemetryConfiguration
        {
            ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000",
            TelemetryChannel = telemetryChannel
        });

        private enum TestEvent
        {
            CheckInCompleted
        }

        private sealed class CapturingTelemetryChannel : ITelemetryChannel
        {
            public List<EventTelemetry> Events { get; } = [];
            public bool? DeveloperMode { get; set; }
            public string EndpointAddress { get; set; } = string.Empty;

            public void Send(ITelemetry item)
            {
                if (item is EventTelemetry eventTelemetry)
                    Events.Add(eventTelemetry);
            }

            public void Flush() { }
            public void Dispose() { }
        }

        private sealed class CapturingCustomEventTracker : ICustomEventTracker
        {
            public List<(string Name, Dictionary<string, string> Properties)> Events { get; } = [];

            public void TrackEvent(string eventName, IDictionary<string, string> properties) =>
                Events.Add((eventName, new Dictionary<string, string>(properties, StringComparer.OrdinalIgnoreCase)));
        }

        private sealed class ThrowingCustomEventTracker : ICustomEventTracker
        {
            public void TrackEvent(string eventName, IDictionary<string, string> properties) => throw new InvalidOperationException("Synthetic tracker failure");
        }
    }
}
