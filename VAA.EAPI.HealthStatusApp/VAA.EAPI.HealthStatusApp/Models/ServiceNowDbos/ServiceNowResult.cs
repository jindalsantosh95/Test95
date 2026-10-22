using System.Text.Json.Serialization;

namespace VAA.EAPI.HealthStatusApp.Models.NewFolder
{
    public class ServiceNowResult
    {
        [JsonPropertyName("requests")]
        public IEnumerable<ServiceNowModel> Requests { get; set; }
    }
}
