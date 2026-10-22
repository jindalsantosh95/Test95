namespace VAA.EAPI.GraphQL.Infrastructure.Entities.SkyMS
{
    public class SkyFlightsPointsClaimRequest
    {
        public string FcNumber { get; set; }
        public string Airline { get; set; }
        public string TicketNumber { get; set; }
        public string FlightNumber { get; set; }
        public string CabinClass { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureFrom { get; set; }
        public string ArrivingAt { get; set; }
        public string SeatNumber { get; set; }
        public string PnrNumber { get; set; }
    }
}
