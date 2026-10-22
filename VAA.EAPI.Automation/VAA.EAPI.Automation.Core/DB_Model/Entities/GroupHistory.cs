using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class GroupHistory
    {
        [JsonProperty("HistoryId")]
        public string HistoryId { get; set; }

        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Include)]
        public string CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Include)]
        public string LastUpdatedOn { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("StartDate")]
        public string StartDate { get; set; }

        [JsonProperty("ExitDate")]
        public string ExitDate { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }
    }
}
