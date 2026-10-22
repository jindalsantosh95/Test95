namespace VAA.EAPI.Automation.Search.Models.OfferSearch
{
    public class BasketCreateRequest
    {
        public List<Person> people { get; set; }
        public FlightSearchRequest flightsearchRequest { get; set; }
    }
}