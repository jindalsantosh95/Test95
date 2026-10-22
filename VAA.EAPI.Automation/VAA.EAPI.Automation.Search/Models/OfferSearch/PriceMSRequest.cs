namespace VAA.EAPI.Automation.Search.Models.OfferSearch
{
    public class PriceMSRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public FlightSearchRequest flightsearchRequest { get; set; }
        public List<CustomerDetails> customerDetails { get; set; }
    }
}