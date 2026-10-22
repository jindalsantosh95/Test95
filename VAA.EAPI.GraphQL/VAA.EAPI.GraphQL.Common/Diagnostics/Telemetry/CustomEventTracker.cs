using System;
using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Diagnostics.Logging.Datadog;

namespace VAA.EAPI.GraphQL.Common.Diagnostics.Telemetry
{
    internal sealed class CustomEventTracker : ICustomEventTracker
    {
        private readonly IEnumerable<ICustomEventSink> datadogSinks;

        public CustomEventTracker(IEnumerable<ICustomEventSink> sinks)
        {
            datadogSinks = sinks ?? throw new ArgumentNullException(nameof(sinks));
        }

        public void TrackEvent(string eventName, IDictionary<string, string> properties)
        {
            foreach (var sink in datadogSinks)
            {
                try
                {
                    sink.TrackEvent(eventName, properties);
                }
                catch
                {
                    DatadogDeliveryMetrics.RecordSinkFailure();
                }
            }
        }
    }
}
