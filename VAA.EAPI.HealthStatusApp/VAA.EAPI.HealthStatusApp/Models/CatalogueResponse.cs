using Microsoft.AspNetCore.Http.Headers;
using System.Net;
using System.Text.Json.Serialization;

namespace VAA.EAPI.HealthStatusApp.Models
{
    public class CatalogueResponse
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("response")]
        public string Response { get; set; }
    }
}
