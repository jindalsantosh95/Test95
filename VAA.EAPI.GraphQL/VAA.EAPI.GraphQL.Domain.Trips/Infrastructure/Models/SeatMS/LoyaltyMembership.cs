using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.Customer;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatMSMembershipDetails")]
    public class LoyaltyMembership
    {
        public string MemberNumber { get; set; }
        public string ProgramName { get; set; }

        public static explicit operator LoyaltyMembership(LoyaltyMemberDetails source)
        {
            if (source == null) return null;

            return new LoyaltyMembership
            {
                MemberNumber = source.MemberNumber,
                ProgramName = source.ProgramName
            };
        }
    }
}