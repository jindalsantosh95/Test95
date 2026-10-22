namespace VAA.EAPI.Automation.Search.Models.OfferSearch
{
    public class SearchOriginDestination
    {
        public string origin { get; set; }
        public string destination { get; set; }
        public string departureDate { get; set; }
        public List<string> connectionAirports { get; set; }
    }
}