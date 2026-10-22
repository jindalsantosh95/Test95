using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class PreferenceAudit
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [JsonProperty("PreferenceUpdatedBy")]
        public string PreferenceUpdatedBy { get; set; }

        [JsonProperty("PreferenceUpdatedOn")]
        public DateTime? PreferenceUpdatedOn { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("VAMarketingPreference")]
        public string? VAMarketingPreference { get; set; }

        [JsonProperty("PreferredPartnerFFP")]
        public string? PreferredPartnerFFP { get; set; }

        [JsonProperty("DoNotCall")]
        public string? DoNotCall { get; set; }

        [JsonProperty("DoNotEmail")]
        public string? DoNotEmail { get; set; }

        [JsonProperty("DoNotSMS")]
        public string? DoNotSMS { get; set; }

        [JsonProperty("Language")]
        public string? Language { get; set; }

        [JsonProperty("_rid")]
        public string Rid { get; set; }

        [JsonProperty("_self")]
        public string Self { get; set; }

        [JsonProperty("_etag")]
        public string Etag { get; set; }

        [JsonProperty("_attachments")]
        public string Attachments { get; set; }

        [JsonProperty("_ts")]
        public int Ts { get; set; }
    }

}
