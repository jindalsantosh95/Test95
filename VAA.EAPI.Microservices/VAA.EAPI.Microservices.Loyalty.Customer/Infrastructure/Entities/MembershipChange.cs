using Newtonsoft.Json;
using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class MembershipChange
    {
        public string SchemaVersion { get; set; }

        public string DocType { get; set; }

        public string PartitionKey { get; set; }

        public string CustomerId { get; set; }

        public string SourceIdentifier { get; set; }

        public string SourceSystem { get; set; }

        public string CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public string LastUpdatedOn { get; set; }

        public string LastUpdatedBy { get; set; }

        public string Active { get; set; }

        public string AppliedRuleId { get; set; }

        public string AppliedPromotionId { get; set; }

        public string ApprovalDate { get; set; }

        public string TierApprovalStatus { get; set; }

        public string EndDate { get; set; }

        public string MemberNumber { get; set; }

        public string OriginalTierEndDate { get; set; }

        public string TierPoints { get; set; }

        public string ProcessDate { get; set; }

        public string ProcessingComment { get; set; }

        public string ProcessingLog { get; set; }

        public string ProcessingStatus { get; set; }

        public string QualifyingPeriodFlag { get; set; }

        public string StartDate { get; set; }

        public string TierCodeManuallyChangedBy { get; set; }

        public string TierClass { get; set; }

        public string TierCode { get; set; }

        public int Revision { get; set; }

        public string Comments { get; set; }

        public string ManualTierChangeReason { get; set; }

        public long QualifyingTierPoints { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Brand { get; set; }

        public string DowngradeProhibitReason { get; set; }
        public string ChangeId { get; set; }


    }
}
