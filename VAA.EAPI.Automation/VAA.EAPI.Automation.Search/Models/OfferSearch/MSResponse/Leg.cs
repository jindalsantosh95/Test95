namespace VAA.EAPI.Automation.Search.Models.OfferSearch.MSResponse
{
    public class Leg
    {
        public ScheduledDepartureTime scheduledDepartureTime { get; set; }
        public string originAirportCode { get; set; }
        public ScheduledArrivalTime scheduledArrivalTime { get; set; }
        public string destinationAirportCode { get; set; }
        public string legDuration { get; set; }
        public bool isDominantLeg { get; set; }
    }
}
