namespace VAA.EAPI.Automation.Search.Models.OfferSearch
{
    public class FlightSearchRequest
    {
        public List<SearchOriginDestination> searchOriginDestinations { get; set; }
        public bool bundleOffer { get; set; }
        public bool flexiDateSearch { get; set; }
        public bool calendarSearch { get; set; }
        public bool nonStopOnly { get; set; }
        public bool refundableOnly { get; set; }
        public bool checkInBaggageAllowance { get; set; }
        public bool carryOnBaggageAllowance { get; set; }
        public string offerID { get; set; }
        public string currentTripIndexId { get; set; }
        public List<CabinType> cabinTypes { get; set; }
        public List<string> fareFamilies { get; set; }
        public bool awardSearch { get; set; }
        public string promoCode { get; set; }
    }
}