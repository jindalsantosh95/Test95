using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS
{
    [GraphQLName("LoyaltyMemberDetailsPayment")]
    public class LoyaltyMember
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MemberNumber { get; set; }
        public string MemberTierCode { get; set; }
    }
}