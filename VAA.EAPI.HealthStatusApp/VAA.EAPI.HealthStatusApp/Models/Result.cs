using Newtonsoft.Json;

namespace VAA.EAPI.HealthStatusApp.Models
{
    public class Result
    {
        [JsonProperty("entries")]
        public Dictionary<string, Entry> Entries { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("totalDuration")]
        public string TotalDuration { get; set; }
    }
}
