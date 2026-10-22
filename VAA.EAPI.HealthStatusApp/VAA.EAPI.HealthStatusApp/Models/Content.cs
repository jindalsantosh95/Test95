using Newtonsoft.Json;

namespace VAA.EAPI.HealthStatusApp.Models
{
    public class Content
    {
        [JsonProperty("headers")]
        public List<Header> Headers { get; set; }
    }
    public class Header
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public List<string> Value { get; set; }
    }
}
