using System.Text.Json.Serialization;

namespace VAA.EAPI.HealthStatusApp.Models.MetricsModels
{
    public class MetricData
    {
        [JsonPropertyName("timeStamp")]
        public DateTime TimeStamp { get; set; }

        [JsonPropertyName("average")]
        public double Average { get; set; }
    }
}
