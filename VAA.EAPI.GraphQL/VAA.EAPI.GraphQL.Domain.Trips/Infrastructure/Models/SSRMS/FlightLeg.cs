namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS
{
    public class FlightLeg
    {
        public int LegId { get; set; }
        public int Ordinal { get; set; }
        public IEnumerable<Airline> OperatingAirlines { get; set; }
        public string FlightNumber { get; set; }
        public DateTime? ScheduledDepartureLocalDateTime { get; set; }
        public DateTime? ScheduledArrivalLocalDateTime { get; set; }
    }

}
