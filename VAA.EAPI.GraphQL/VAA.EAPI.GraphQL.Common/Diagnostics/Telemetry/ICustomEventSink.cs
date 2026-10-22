using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Common.Diagnostics.Telemetry
{
    internal interface ICustomEventSink
    {
        void TrackEvent(string eventName, IDictionary<string, string> properties);
    }
}
