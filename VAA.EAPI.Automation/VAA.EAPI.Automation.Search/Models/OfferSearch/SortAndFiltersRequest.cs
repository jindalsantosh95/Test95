namespace VAA.EAPI.Automation.Search.Models.OfferSearch
{
    public class SortAndFiltersRequest
    {
        public FlightSearchRequest flightSearchRequest { get; set; }
        public List<CustomerDetails> customerDetails { get; set; }
        public string sortRequest { get; set; }
        public Filters filters { get; set; }
        public string basketId { get; set; }
    }
}