using System.Text.Json.Serialization;
using VAA.EAPI.HealthStatusApp.Models.ViewModels;

namespace VAA.EAPI.HealthStatusApp.Models.MetricsModels
{
    public class CosmosDbMetricsModel
    {
        [JsonPropertyName("serviceName")]
        public string ServiceName { get; set; }
        [JsonPropertyName("cosmosDbMetrics")]
        public IEnumerable<CosmosDbMetricsData> CosmosDbMetrics { get; set; }
    }
}
