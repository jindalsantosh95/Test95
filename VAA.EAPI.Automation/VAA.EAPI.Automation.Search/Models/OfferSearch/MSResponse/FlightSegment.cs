namespace VAA.EAPI.Automation.Search.Models.OfferSearch.MSResponse
{
    public class FlightSegment
    {
        public string odRefId { get; set; }
        public string segmentId { get; set; }
        public string airCraft { get; set; }
        public string marketingCarrier { get; set; }
        public string marketingFlightNumber { get; set; }
        public string operatingCarrier { get; set; }
        public string operatingFlightNumber { get; set; }
        public string originAirportCode { get; set; }
        public string classOfService { get; set; }
        public string originCityCode { get; set; }
        public string destinationAirportCode { get; set; }
        public string destinationCityCode { get; set; }
        public ScheduledArrivalTime scheduledArrivalTime { get; set; }
        public ScheduledDepartureTime scheduledDepartureTime { get; set; }
        public string flightDuration { get; set; }
        public List<Leg> legs { get; set; }
        public string connectionOrStopOver { get; set; }
    }
}
