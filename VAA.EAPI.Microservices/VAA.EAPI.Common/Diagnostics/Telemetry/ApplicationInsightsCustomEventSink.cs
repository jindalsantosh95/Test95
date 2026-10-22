using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;

namespace VAA.EAPI.Common.Diagnostics.Telemetry
{
    internal sealed class ApplicationInsightsCustomEventSink : ICustomEventSink
    {
        private readonly TelemetryClient telemetryClient;

        public ApplicationInsightsCustomEventSink(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        public void TrackEvent(string eventName, IDictionary<string, string> properties) => telemetryClient.TrackEvent(eventName, properties);
    }
}
