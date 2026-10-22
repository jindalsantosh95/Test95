namespace VAA.EAPI.Automation.Search.Models.OfferSearch
{
    public class OfferMSRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public bool FeatureFlagUseGraphQl { get; set; }
        public FlightSearchRequest flightsearchRequest { get; set; }
        public List<CustomerDetails> customerDetails { get; set; }
        public LoyaltyMember loyaltyMember { get; set; }
    }
}