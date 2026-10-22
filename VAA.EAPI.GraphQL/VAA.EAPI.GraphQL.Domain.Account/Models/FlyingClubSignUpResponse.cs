using HotChocolate;
using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    [GraphQLName("FlyingClubSignUpResponse")]
    public class FlyingClubSignUpResponse
    {
        public string? CustomerId { get; set; }

        public MemberDetails? MemberDetails { get; set; }
        public List<ErrorDetails>? Errors { get; set; }
    }
}






