namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL
{
    public class CardLinkingAuditResponse
    {
        public string statusEnhancementCode { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public string sourceIdentifier { get; set; }
        public string partnerCode { get; set; }
        public string cardLinkingDocId { get; set; }
        public string cardLinkingUpdatedBy { get; set; }
        public DateTime? cardLinkingUpdatedOn { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string customerId { get; set; }
    }

}
