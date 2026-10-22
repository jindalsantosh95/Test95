using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderSearch
{
    [GraphQLName("Retailing.LoyaltyMemberDetails")]
    public class LoyaltyMemberDetails
    {
        public string MemberType { get; set; }
        public string ProgramName { get; set; }
        public string MemberNumber { get; set; }
        public string Status { get; set; }
        public string ProgramCode { get; set; }
    }
}