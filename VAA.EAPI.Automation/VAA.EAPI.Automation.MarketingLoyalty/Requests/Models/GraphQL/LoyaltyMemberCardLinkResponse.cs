namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL
{
    public class LoyaltyMemberCardLinkResponse
    {
        public string documentId { get; set; }
        public string partnerCode { get; set; }
        public string partnerName { get; set; }
        public string cardType { get; set; }
        public DateTime? linkedDate { get; set; }
        public DateTime? endDate { get; set; }
        public DateTime? anniversaryDate { get; set; }
        public string pan { get; set; }
    }
}
