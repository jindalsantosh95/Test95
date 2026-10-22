using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    public class CreditDetails
    {
        public string CreditItemId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string CreditValue { get; set; }
        public string PromotionRuleActionId { get; set; }
        public string PointType { get; set; }
        public string Comments { get; set; }
        public string PromotionId { get; set; }
        public string PointSubType { get; set; }
        public string QualifyingFlag { get; set; }
        public string CreditValueUsed { get; set; }
    }
}
