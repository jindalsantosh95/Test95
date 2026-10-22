using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class MembershipChange
    {
        [JsonProperty("SchemaVersion")]
        public string SchemaVersion { get; set; }



        [JsonProperty("DocType")]
        public string DocType { get; set; }



        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }



        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }



        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }



        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }



        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedOn { get; set; }



        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }



        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdatedOn { get; set; }



        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }



        [JsonProperty("Active")]
        public string Active { get; set; }



        [JsonProperty("AppliedRuleId")]
        public string AppliedRuleId { get; set; }



        [JsonProperty("AppliedPromotionId")]
        public string AppliedPromotionId { get; set; }



        [JsonProperty("ApprovalDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ApprovalDate { get; set; }



        [JsonProperty("TierApprovalStatus")]
        public string TierApprovalStatus { get; set; }



        [JsonProperty("EndDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? EndDate { get; set; }



        [JsonProperty("MemberNumber")]
        public string MemberNumber { get; set; }



        [JsonProperty("OriginalTierEndDate")]
        public string OriginalTierEndDate { get; set; }



        [JsonProperty("TierPoints")]
        public string TierPoints { get; set; }



        [JsonProperty("VSTierPoints")]
        public string VsTierPoints { get; set; }



        [JsonProperty("ProcessDate")]
        public string ProcessDate { get; set; }



        [JsonProperty("ProcessingComment")]
        public string ProcessingComment { get; set; }



        [JsonProperty("ProcessingLog")]
        public string ProcessingLog { get; set; }



        [JsonProperty("ProcessingStatus")]
        public string ProcessingStatus { get; set; }



        [JsonProperty("QualifyingPeriodFlag")]
        public string QualifyingPeriodFlag { get; set; }



        [JsonProperty("StartDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? StartDate { get; set; }



        [JsonProperty("TierCodeManuallyChangedBy")]
        public string TierCodeManuallyChangedBy { get; set; }



        [JsonProperty("TierClass")]
        public string TierClass { get; set; }



        [JsonProperty("TierCode")]
        public string TierCode { get; set; }



        [JsonProperty("TransactionId")]
        public string TransactionId { get; set; }



        [JsonProperty("Comments")]
        public string Comments { get; set; }



        [JsonProperty("ManualTierChangeReason")]
        public string ManualTierChangeReason { get; set; }



        [JsonProperty("QualifyingTierPoints")]
        public string QualifyingTierPoints { get; set; }



        [JsonProperty("DBLastUpdatedDate")]
        public string DbLastUpdatedDate { get; set; }



        [JsonProperty("DBLastUpdatedSource")]
        public string DbLastUpdatedSource { get; set; }


        [JsonProperty("id")]
        public string Id { get; set; }


        [JsonProperty("Brand")]
        public string Brand { get; set; }


        [JsonProperty("Revision")]
        public double? Revision { get; set; }


        [JsonProperty("_etag")]
        public string _etag { get; set; }

    }
}
