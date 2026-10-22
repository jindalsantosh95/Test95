using System;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine
{
    public class MemberShipCardDetails
    {
        public string Active { get; set; }
        public string CardStatusCode { get; set; }
        public string CardType { get; set; }
        public string TierCode { get; set; }
        public DateTime? RequestedDate { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? CardExpiryDate { get; set; }
        public string CardReplacementReason { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
