using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.Customer
{
    [GraphQLName("GraphqlTripsLoyaltyMemberDetails")]
    public class LoyaltyMemberDetails
    {
        public string MemberType { get; set; }
        public string ProgramName { get; set; }
        public string MemberNumber { get; set; }
        public string MemberTierCode { get; set; }
        public string Status { get; set; }
        public string ProgramCode { get; set; }
        public bool PayWithPoints { get; set; }
    }
}
