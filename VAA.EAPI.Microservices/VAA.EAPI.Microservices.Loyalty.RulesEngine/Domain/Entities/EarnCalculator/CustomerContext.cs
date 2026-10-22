namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.EarnCalculator
{
    public class CustomerContext
    {
        public string CurrentStatus { get; set; }
        public string MemberType { get; set; }
        public string CurrentTier { get; set; }
        public string CurrentTierStartDate { get; set; }
        public string CampaignOrPromotionCode { get; set; }
    }
}
