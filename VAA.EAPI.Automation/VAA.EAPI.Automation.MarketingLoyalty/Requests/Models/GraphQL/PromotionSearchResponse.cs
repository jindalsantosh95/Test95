namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL
{
    public class PromotionSearchResponse
    {
        public DateTime? startDate { get; set; }
        public string promotionName { get; set; }
        public string promotionId { get; set; }
        public string partnerName { get; set; }
        public string partnerId { get; set; }
        public DateTime? endDate { get; set; }
        public string campaignLinked { get; set; }
        public string active { get; set; }
    }
}
