namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch
{
    public class FlightRequestCriteria
    {
        public string currentTripIndexId { get; set; }
        public string selectedOfferId { get; set; }
        public bool calendarSearch { get; set; }
        public List<SearchOriginDestination> searchOriginDestination { get; set; }
    }
}