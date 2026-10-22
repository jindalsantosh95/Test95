using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class OtherAudit
    {

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [JsonProperty("DataUpdatedBy")]
        public string DataUpdatedBy { get; set; }

        [JsonProperty("DataUpdatedOn")]
        public DateTime? DataUpdatedOn { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("AuditCategory")]
        public string AuditCategory { get; set; }

        [JsonProperty("AuditSubCategory")]
        public string AuditSubCategory { get; set; }

        [JsonProperty("RelationshipType")]
        public string RelationshipType { get; set; }

        [JsonProperty("AuditData")]
        public string AuditData { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("_rid")]
        public string Rid { get; set; }

        [JsonProperty("_self")]
        public string Self { get; set; }

        [JsonProperty("_etag")]
        public string Etag { get; set; }

        [JsonProperty("_attachments")]
        public string Attachments { get; set; }

        [JsonProperty("_ts")]
        public int? Ts { get; set; }
    }
}
