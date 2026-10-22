using Newtonsoft.Json;
namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{

    public class DocumentDetail
    {
        [JsonProperty("Active", NullValueHandling = NullValueHandling.Ignore)]
        public string Active { get; set; }

        [JsonProperty("CaptureCannel")]
        public string CaptureCannel { get; set; }

        [JsonProperty("CommsType")]
        public string CommsType { get; set; }

        [JsonProperty("DBLastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DbLastUpdatedOn { get; set; }

        [JsonProperty("DBLastUpdatedSrc")]
        public string DbLastUpdatedSrc { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdatedOn { get; set; }

        [JsonProperty("Persona")]
        public string Persona { get; set; }

        [JsonProperty("PreferenceId")]
        public string PreferenceId { get; set; }

        [JsonProperty("PreferenceSubType")]
        public string PreferenceSubType { get; set; }

        [JsonProperty("PreferenceType")]
        public string PreferenceType { get; set; }

        [JsonProperty("PreferenceValue")]
        public string PreferenceValue { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }
    }

}
