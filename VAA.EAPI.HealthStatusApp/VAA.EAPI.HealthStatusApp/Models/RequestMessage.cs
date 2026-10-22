using Newtonsoft.Json;

namespace VAA.EAPI.HealthStatusApp.Models
{
    public class RequestMessage
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("method")]
        public Method Method { get; set; }

        [JsonProperty("requestUri")]
        public string RequestUri { get; set; }

        [JsonProperty("headers")]
        public List<Header> Headers { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, object> Properties { get; set; }

        [JsonProperty("options")]
        public Dictionary<string, object> Options { get; set; }
    }

    public class Method
    {
        [JsonProperty("method")]
        public string MethodName { get; set; }
    }
}
