using System.Text.Json.Serialization;

namespace VAA.EAPI.HealthStatusApp.Models.MetricsModels
{
    public class MetricValue
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("name")]
        public MetricName Name { get; set; }

        [JsonPropertyName("displayDescription")]
        public string DisplayDescription { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }

        [JsonPropertyName("timeseries")]
        public TimeSeries[] Timeseries { get; set; }

        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; }
    }
}
