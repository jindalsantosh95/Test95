namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public class LoyaltyMemberDetails
    {
        public string? CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public FQTMembership MembershipDetails { get; set; } = new();
    }
}
