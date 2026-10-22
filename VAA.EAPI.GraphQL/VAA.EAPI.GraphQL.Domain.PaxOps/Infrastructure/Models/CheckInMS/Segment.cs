using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS
{
    public class Segment
    {
        public string SegmentId { get; set; }
        public int Ordinal { get; set; }
        public FlightCarrier OperatingCarrier { get; set; }
        public FlightCarrier MarketingCarrier { get; set; }
        public LocalDateTime ScheduledDepartureLocalDateTime { get; set; }
        public LocalDateTime ScheduledDepartureUtcDateTime { get; set; }
        public LocalDateTime ScheduledArrivalLocalDateTime { get; set; }
        public LocalDateTime ScheduledArrivalUtcDateTime { get; set; }
        public AirportAndTerminal Origin { get; set; }
        public AirportAndTerminal Destination { get; set; }
        public IEnumerable<Leg> Legs { get; set; }
    }
}