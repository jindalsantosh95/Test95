using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class AwardAudit
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [JsonProperty("AwardUpdatedBy")]
        public string AwardUpdatedBy { get; set; }

        [JsonProperty("AwardUpdatedOn")]
        public DateTime? AwardUpdatedOn { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("AwardNumber")]
        public string AwardNumber { get; set; }

        [JsonProperty("AwardName")]
        public string AwardName { get; set; }

        [JsonProperty("StartDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("EndDate")]
        public DateTime? EndDate { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("UsedDate")]
        public DateTime? UsedDate { get; set; }

        [JsonProperty("AwardId")]
        public string AwardId { get; set; }

        [JsonProperty("AwardProductId")]
        public string AwardProductId { get; set; }

        [JsonProperty("GiftedAwardDate")]
        public DateTime? GiftedAwardDate { get; set; }

        [JsonProperty("GiftedFromCustomerId")]
        public string GiftedFromCustomerId { get; set; }

        [JsonProperty("GiftedToCustomerId")]
        public string GiftedToCustomerId { get; set; }

        [JsonProperty("GiftedFromMemberNumber")]
        public string GiftedFromMemberNumber { get; set; }

        [JsonProperty("GiftedToMemberNumber")]
        public string GiftedToMemberNumber { get; set; }

        [JsonProperty("PreviousStatus")]
        public string PreviousStatus { get; set; }

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

