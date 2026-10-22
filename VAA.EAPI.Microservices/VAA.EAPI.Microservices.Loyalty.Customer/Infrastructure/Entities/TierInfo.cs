namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class TierInfo : Customers
    {
        public string CurrentTier { get; set; }
        public string PreviousTier { get; set; }
        public string JoinDatetime { get; set; }
        public string TierChangeDatetime { get; set; }
    }
}
