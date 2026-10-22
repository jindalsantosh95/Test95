namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain
{
    public class Customer
    {
        public string CustomerId { get; set; }
        public string LoyaltyNumber { get; set; }
        public string PreferredFirstName { get; set; }
        public string PreferredLastName { get; set; }
        public string MemberTier { get; set; }

    }
}
