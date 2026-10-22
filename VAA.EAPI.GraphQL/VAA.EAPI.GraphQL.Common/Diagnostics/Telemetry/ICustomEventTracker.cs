using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Common.Diagnostics.Telemetry
{
    /// <summary>
    /// Tracks the reviewed Datadog projection of a business custom event.
    /// Application Insights owns its separate projection in the calling telemetry service.
    /// </summary>
    public interface ICustomEventTracker
    {
        void TrackEvent(string eventName, IDictionary<string, string> properties);
    }
}
