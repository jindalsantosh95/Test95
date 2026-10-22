namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public class BasketLoyaltyMemberDetails
    {
        public string? CustomerId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public FQTMembership? MembershipDetails { get; set; }
    }
}
