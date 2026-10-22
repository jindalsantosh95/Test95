namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    public class BasketLoyaltyMemberDetails
    {
        [HotChocolate.GraphQLIgnore]
        public string? CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MemberEmailAddress { get; set; }
        public string MemberDateOfBirth { get; set; }
        public FQTMembership MembershipDetails { get; set; } = new();
        public BasketLoyaltyMemberAddress MemberAddress { get; set; }
    }
}
