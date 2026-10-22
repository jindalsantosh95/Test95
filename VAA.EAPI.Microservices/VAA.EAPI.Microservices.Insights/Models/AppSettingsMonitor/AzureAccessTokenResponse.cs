using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Insights.Models.AppSettingsMonitor
{
    public sealed class AzureAccessTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
