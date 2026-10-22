using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.Web.Common.Diagnostics.Logging.Datadog
{
    internal sealed class DatadogLogEntry
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; init; }

        [JsonProperty("message")]
        public string Message { get; init; }

        [JsonProperty("status")]
        public string Status { get; init; }

        [JsonProperty("service")]
        public string Service { get; init; }

        [JsonProperty("ddsource")]
        public string Source { get; init; }

        [JsonProperty("ddtags", NullValueHandling = NullValueHandling.Ignore)]
        public string Tags { get; init; }

        [JsonExtensionData]
        public IDictionary<string, object> Properties { get; init; }
    }
}
