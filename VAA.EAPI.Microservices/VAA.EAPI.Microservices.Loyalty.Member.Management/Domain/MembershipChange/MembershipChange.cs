using System;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.MembershipChange
{
    public class MembershipChange
    {
        public string Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string CustomerId { get; set; }
        public string ChangeId { get; set; }
        public string Active { get; set; }
        public string AppliedRuleId { get; set; }
        public string AppliedPromotionId { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string TierApprovalStatus { get; set; }
        public DateTime EndDate { get; set; }
        public string MemberNumber { get; set; }
        public string OriginalTierEndDate { get; set; }
        public string TierPoints { get; set; }
        public DateTime? ProcessDate { get; set; }
        public string ProcessingComment { get; set; }
        public string ProcessingLog { get; set; }
        public string ProcessingStatus { get; set; }
        public string QualifyingPeriodFlag { get; set; }
        public DateTime StartDate { get; set; }
        public string TierCodeManuallyChangedBy { get; set; }
        public string TierClass { get; set; }
        public string TierCode { get; set; }
        public string Comments { get; set; }
        public string ManualTierChangeReason { get; set; }
        public string QualifyingTierPoints { get; set; }
        public string DowngradeProhibitReason { get; set; }
        public string Revision { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
    }
}
