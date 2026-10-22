using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class CardLinkingAudit
    {

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("StatusEnhancementCode")]
        public string StatusEnhancementCode { get; set; }

        [JsonProperty("StartDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("EndDate")]
        public DateTime? EndDate { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("CardLinkingUpdatedBy")]
        public string CardLinkingUpdatedBy { get; set; }

        [JsonProperty("CardLinkingUpdatedOn")]
        public DateTime? CardLinkingUpdatedOn { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("CardLinkingDocID")]
        public string CardLinkingDocID { get; set; }

        [JsonProperty("PartnerCode")]
        public string PartnerCode { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("_rid")]
        public string _rid { get; set; }

        [JsonProperty("_self")]
        public string _self { get; set; }

        [JsonProperty("_etag")]
        public string _etag { get; set; }

        [JsonProperty("_attachments")]
        public string _attachments { get; set; }

        [JsonProperty("_ts")]
        public int? _ts { get; set; }
    }
}
