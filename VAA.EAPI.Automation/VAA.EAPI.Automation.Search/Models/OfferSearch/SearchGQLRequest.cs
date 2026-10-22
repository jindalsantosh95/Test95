namespace VAA.EAPI.Automation.Search.Models.OfferSearch
{
    public class SearchGQLRequest
    {
        public FlightSearchRequest flightSearchRequest { get; set; }
        public List<CustomerDetails> customerDetails { get; set; }
    }
}