using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatsCustomer")]
    public class Customer
    {
        public string Id { get; set; }
        public string Ptc { get; set; }
        public LoyaltyMembership LoyaltyMemberDetails { get; set; }

        public static explicit operator Customer((Person person, string id) source)
        {
            var (person, id) = source;

            return new()
            {
                Id = id,
                Ptc = person.Type,
                LoyaltyMemberDetails = person.MembershipDetails != null ? new() { MemberNumber = person.MembershipDetails.FQTNumber, ProgramName = person.MembershipDetails.FQTProgramme } : null
            };
        }
    }
}