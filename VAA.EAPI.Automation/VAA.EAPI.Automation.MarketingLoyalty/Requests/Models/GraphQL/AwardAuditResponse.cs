namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL
{
    public class AwardAuditResponse
    {
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string awardUpdatedBy { get; set; }
        public DateTime? awardUpdatedOn { get; set; }
        public string customerId { get; set; }
        public string sourceIdentifier { get; set; }
        public string awardNumber { get; set; }
        public string awardName { get; set; }
        public DateTime? startDate { get; set; }
        public string status { get; set; }
        public string awardId { get; set; }
        public string awardProductId { get; set; }
        public string giftedFromCustomerId { get; set; }
        public string giftedFromMemberNumber { get; set; }
        public string giftedToCustomerId { get; set; }
        public string giftedToMemberNumber { get; set; }
        public string previousStatus { get; set; }
        public string id { get; set; }

    }
}
