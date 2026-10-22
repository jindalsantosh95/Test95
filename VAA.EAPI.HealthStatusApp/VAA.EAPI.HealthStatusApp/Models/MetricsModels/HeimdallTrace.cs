using System.Text.Json.Serialization;

namespace VAA.EAPI.HealthStatusApp.Models.MetricsModels
{
    public class HeimdallTrace
    {
        [JsonPropertyName("timeStamp")]
        public DateTime TimeStamp { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
