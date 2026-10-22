using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public sealed class Basket
    {
        public Guid BasketId { get; set; }
        public IEnumerable<People> People { get; set; }
        public Address AddressDetails { get; set; }
        public ContactDetails ContactDetails { get; set; }
        public TermsAndConditions TermsAndConditions { get; set; }
        public IEnumerable<string> OfferIds { get; set; }
        public IEnumerable<BookingJourneyOfferDetails> BookingJourneyOfferDetails { get; set; }
        public BasketLoyaltyMemberDetails CustomerProfile { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; }
        public string PaymentLink { get; set; }
        public string ResourceType { get; set; }
        public FlightsearchRequest FlightsearchRequest { get; set; }
        public BasketFCEarnPoints EarnPoints { get; set; }
        public FcPointsPerTier EarnPointPerTier { get; set; }
        public string FlightSummary { get; set; }
        public string SummaryLink { get; set; }
        public string MetaPartnerId { get; set; }
        public bool? HasSufficientPoints { get; set; }
        public string? VoucherPoints { get; set; }
        public string? PointsToPay { get; set; }
        public bool VoucherApplied { get; set; }
        public Models.BFFSummary.VoucherRedemptionDetails? VoucherDetails { get; set; }
        public Models.BFFSummary.VoucherRedemptionSelection? VoucherRedemptionSelection { get; set; }


        public static explicit operator Basket(FlightOfferBaseRequest source)
        {
            if (source == null)
                return null;

            return new()
            {
                FlightsearchRequest = (Models.Basket.FlightsearchRequest)source.FlightSearchRequest,
                People = source.CustomerDetails.Select(c => (Models.Basket.People)c)
            };
        }

        public string? GetPassengerCounts()
        {
            if (People == null || People.Any() == false)
            {
                return null;
            }
            var numAdults = People.Count(c => c.Type == PtcEnums.PtcTypeEnum.ADT.ToString());
            var numTeens = People.Count(c => c.Type == PtcEnums.PtcTypeEnum.GBE.ToString());
            var numChildren = People.Count(c => c.Type == PtcEnums.PtcTypeEnum.CNN.ToString());
            var numInfants = People.Count(c => c.Type == PtcEnums.PtcTypeEnum.INF.ToString());
            return $"a{numAdults}t{numTeens}c{numChildren}i{numInfants}";
        }
    }
}
