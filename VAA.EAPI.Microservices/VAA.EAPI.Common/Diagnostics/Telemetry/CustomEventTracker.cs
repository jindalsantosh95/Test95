using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VAA.EAPI.Common.Diagnostics.Telemetry
{
    internal sealed class CustomEventTracker : ICustomEventTracker
    {
        private readonly IEnumerable<ICustomEventSink> sinks;

        public CustomEventTracker(IEnumerable<ICustomEventSink> sinks)
        {
            this.sinks = sinks ?? throw new ArgumentNullException(nameof(sinks));
        }

        public void TrackEvent(string eventName, IDictionary<string, string> properties)
        {
            foreach (var sink in sinks)
            {
                try
                {
                    sink.TrackEvent(eventName, properties);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Custom event delivery failed: {exception.GetType().Name}");
                }
            }
        }
    }
}
