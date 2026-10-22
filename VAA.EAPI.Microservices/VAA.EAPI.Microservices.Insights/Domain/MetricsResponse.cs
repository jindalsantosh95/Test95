using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Insights.Domain
{
    public class MetricsResponse
    {
        [JsonPropertyName("cost")]
        public int Cost { get; set; }

        [JsonPropertyName("timespan")]
        public string Timespan { get; set; }

        [JsonPropertyName("interval")]
        public string Interval { get; set; }

        [JsonPropertyName("value")]
        public MetricValue[] Value { get; set; }

        [JsonPropertyName("namespace")]
        public string Namespace { get; set; }

        [JsonPropertyName("resourceregion")]
        public string ResourceRegion { get; set; }
    }
}
