using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Party
    {
        public TravelAgency TravelAgency { get; set; }
        public CorporateAccount CorporateAccount { get; set; }
        public LoyaltyMember LoyaltyMember { get; set; }
        public GuestShopper GuestShopper { get; set; }
        public ReservationAgent ReservationAgent { get; set; }

        public static explicit operator Party(LoyaltyMemberDetails source)
        {
            if (source == null) return null;
            return new()
            {
                LoyaltyMember = new()
                {
                    LoyaltyMemberId = source.MemberNumber,
                    LoyaltyProgramTierCode = source.MemberTierCode,
                    PayWithMilesEligible = source.PayWithPoints
                }
            };
        }
    }
}
