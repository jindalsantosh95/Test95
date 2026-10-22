using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.PaymentRequest
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