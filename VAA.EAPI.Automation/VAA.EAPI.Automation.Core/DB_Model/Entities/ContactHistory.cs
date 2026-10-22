using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class ContactHistory
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("SchemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("ContactId")]
        public string ContactId { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("LastUpdatedOn")]
        public DateTime? LastUpdatedOn { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("ContactValue")]
        public string ContactValue { get; set; }

        [JsonProperty("ContactType")]
        public string ContactType { get; set; }

        [JsonProperty("ContactSubType")]
        public string ContactSubType { get; set; }

        [JsonProperty("Usage")]
        public string Usage { get; set; }

        [JsonProperty("Valid")]
        public string Valid { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("CountryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("CountryWithCode")]
        public string CountryWithCode { get; set; }

        [JsonProperty("Primary")]
        public string Primary { get; set; }

        [JsonProperty("_etag")]
        public string _etag { get; set; }

        [JsonProperty("_ts")]
        public int? _ts { get; set; }

    }
}
