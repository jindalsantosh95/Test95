using System.Text.Json.Serialization;

namespace VAA.EAPI.HealthStatusApp.Models.MetricsModels
{
    public class MetricName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("localizedValue")]
        public string LocalizedValue { get; set; }
    }
}
