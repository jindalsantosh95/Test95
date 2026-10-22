using Newtonsoft.Json;

namespace VAA.EAPI.HealthStatusApp.Models
{
    public class Response
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("content")]
        public Content Content { get; set; }

        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("reasonPhrase")]
        public string ReasonPhrase { get; set; }

        [JsonProperty("headers")]
        public List<Header> Headers { get; set; }

        [JsonProperty("trailingHeaders")]
        public List<Header> TrailingHeaders { get; set; }

        [JsonProperty("requestMessage")]
        public RequestMessage RequestMessage { get; set; }
    }
}
