namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.Transaction
{
    public class Creditdetail
    {
        public string CreditItemId { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string CreditValue { get; set; }
        public string PromotionRuleActionId { get; set; }
        public string PointType { get; set; }
        public string Comments { get; set; }
        public string PromotionId { get; set; }
        public string PointSubType { get; set; }
        public string QualifyingFlag { get; set; }
        public string CreditValueUsed { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
    }
}
