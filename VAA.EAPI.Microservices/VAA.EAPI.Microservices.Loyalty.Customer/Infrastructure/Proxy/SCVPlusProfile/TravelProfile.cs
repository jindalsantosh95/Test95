namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class TravelProfile
    {
        public enum ReasonForTravelEnum
        {
            Business = 1,
            Leisure = 2,
            Both = 3
        }
        public ReasonForTravelEnum? ReasonForTravel { get; set; }

        public TravelProfile(ReasonForTravelEnum? reasonForTravel = default, string flyingClubComms = default, string vLanguagePreference = default, string virginGroupComms = default)
        {
            ReasonForTravel = reasonForTravel;
            FlyingClubComms = flyingClubComms;
            VLanguagePreference = vLanguagePreference;
            VirginGroupComms = virginGroupComms;
        }

        public string FlyingClubComms { get; set; }
        public string VLanguagePreference { get; set; }
        public string VirginGroupComms { get; set; }


    }
}
