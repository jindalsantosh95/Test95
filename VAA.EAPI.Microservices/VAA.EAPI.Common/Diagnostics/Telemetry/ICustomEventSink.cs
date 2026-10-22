using System.Collections.Generic;

namespace VAA.EAPI.Common.Diagnostics.Telemetry
{
    internal interface ICustomEventSink
    {
        void TrackEvent(string eventName, IDictionary<string, string> properties);
    }
}
