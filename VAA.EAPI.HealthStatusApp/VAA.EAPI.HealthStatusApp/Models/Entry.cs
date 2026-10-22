using Newtonsoft.Json;

namespace VAA.EAPI.HealthStatusApp.Models
{
    public class Entry
    {
        [JsonProperty("data")]
        public EntryData Data { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("exception", NullValueHandling = NullValueHandling.Ignore)]
        public ExceptionDetail Exception { get; set; }
    }
}
