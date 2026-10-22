using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class MemberTier
    {
        public MemberTier(string updatedBy = default, DateTime? updated = default, DateTime? tierStartDate = default, DateTime? tierEndDate = default, string revision = default, string issueReason = default, string tierName = default, string appliedRuleType = default)
        {
            UpdatedBy = updatedBy;
            Updated = updated;
            TierStartDate = tierStartDate;
            TierEndDate = tierEndDate;
            Revision = revision;
            IssueReason = issueReason;
            TierName = tierName;
            AppliedRuleType = appliedRuleType;
        }

        public string UpdatedBy { get; set; }
        public DateTime? Updated { get; set; }
        public DateTime? TierStartDate { get; set; }
        public DateTime? TierEndDate { get; set; }
        public string Revision { get; set; }
        public string IssueReason { get; set; }
        public string TierName { get; set; }
        public string AppliedRuleType { get; set; }
    }
}
