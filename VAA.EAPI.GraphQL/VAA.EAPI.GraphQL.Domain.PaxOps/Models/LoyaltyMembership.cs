using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:LoyaltyMembership")]
    public class LoyaltyMembership
    {
        public string ProgrammeCode { get; set; }

        [GraphQLDescription("Loyalty membership number.")]
        public string Number { get; set; }

        public static explicit operator LoyaltyMembership(Infrastructure.Models.CheckInMS.LoyaltyMembership source) => new()
        {
            ProgrammeCode = source.ProgrammeCode,
            Number = source.Number
        };
    }
}
