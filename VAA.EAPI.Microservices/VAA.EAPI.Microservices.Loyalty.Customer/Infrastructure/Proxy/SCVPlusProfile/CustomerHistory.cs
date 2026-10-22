using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class CustomerHistory
    {
        public CustomerHistory(Contact contactDetails = default, List<Address> addressDetails = default, List<MemberTier> memberTier = default, List<MembershipCard> membershipCard = default, List<Fulfilment> fulfilment = default, LoyaltyMember loyaltyMember = default, TravelProfile travelProfile = default)
        {
            ContactDetails = contactDetails;
            AddressDetails = addressDetails;
            MemberTier = memberTier;
            MembershipCard = membershipCard;
            Fulfilment = fulfilment;
            LoyaltyMember = loyaltyMember;
            TravelProfile = travelProfile;
        }

        public Contact ContactDetails { get; set; }
        public List<Address> AddressDetails { get; set; }
        public List<MemberTier> MemberTier { get; set; }
        public List<MembershipCard> MembershipCard { get; set; }
        public List<Fulfilment> Fulfilment { get; set; }
        public LoyaltyMember LoyaltyMember { get; set; }
        public TravelProfile TravelProfile { get; set; }
    }
}
