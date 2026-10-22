using System.Text.Json.Serialization;

namespace VAA.EAPI.HealthStatusApp.Models.NewFolder
{
    public class ServiceNowResponse
    {
        [JsonPropertyName("result")]
        public ServiceNowResult Result { get; set; }

    }
}
