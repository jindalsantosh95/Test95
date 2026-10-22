using System.Collections.Generic;

namespace VAA.EAPI.Common.Diagnostics.Telemetry
{
    /// <summary>
    /// Tracks a business-safe application custom event with each configured telemetry destination.
    /// </summary>
    public interface ICustomEventTracker
    {
        void TrackEvent(string eventName, IDictionary<string, string> properties);
    }
}
