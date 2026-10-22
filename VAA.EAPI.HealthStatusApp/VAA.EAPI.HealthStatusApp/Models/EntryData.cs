using Newtonsoft.Json;
using VAA.EAPI.HealthStatusApp.Helpers;

namespace VAA.EAPI.HealthStatusApp.Models
{
    public class EntryData
    {
        [JsonProperty("endPoint", NullValueHandling = NullValueHandling.Ignore)]
        public string EndPoint { get; set; }

        [JsonProperty("expectedHttpStatusCode")]
        public int? ExpectedHttpStatusCode { get; set; }

        [JsonProperty("StatusCode")]
        [JsonConverter(typeof(StringToIntConverter))]
        public int? StatusCode { get; set; }
    }
}
