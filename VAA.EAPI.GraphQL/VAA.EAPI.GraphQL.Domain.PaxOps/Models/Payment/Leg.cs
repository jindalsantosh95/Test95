namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Payment
{
    public class Leg
    {
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public UtcAndLocalDateTime ScheduledDepartureTime { get; set; }
        public UtcAndLocalDateTime ScheduledArrivalTime { get; set; }
    }
}
