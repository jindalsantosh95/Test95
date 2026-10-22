using System.Text.Json.Serialization;

namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class CosmosDbMetricsData
    {
        [JsonPropertyName("timeStamp")]
        public string Timestamp { get; set; }
        [JsonPropertyName("maxNormalizedRUConsumptionPercentage")]
        public int MaxNormalizedRUConsumptionPercentage { get; set; }

    }
}
