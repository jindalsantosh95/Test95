
using System;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine
{
    public class MembershipChangeDocument
    {
        public string CustomerId { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Active { get; set; }
        public string AppliedRuleId { get; set; }
        public string AppliedPromotionId { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string TierApprovalStatus { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string TierCode { get; set; }
        public string MemberNumber { get; set; }

    }
}
