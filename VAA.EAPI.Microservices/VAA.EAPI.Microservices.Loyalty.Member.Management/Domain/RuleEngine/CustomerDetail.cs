namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine
{
    public class CustomerDetail
    {
        public string MemberNumber { get; set; }

        public string TierCode { get; set; }

        public string MemberType { get; set; }

        public string MemberStatus { get; set; }

        public string CampaignOrPromotionCode { get; set; }

        public string CurrentTierStartDate { get; set; }
    }
}
