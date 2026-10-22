namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL
{
    public class AwardDetails
    {
        public string awardId { get; set; }

        public string awardNumber { get; set; }

        public string productName { get; set; }

        public DateTime? startDate { get; set; }

        public DateTime? expiryDate { get; set; }

    }
}
