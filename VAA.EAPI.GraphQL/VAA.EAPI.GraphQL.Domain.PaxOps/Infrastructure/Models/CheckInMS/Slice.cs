using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS
{
    public class Slice
    {
        public string SliceId { get; set; }
        public LocalDateTime ScheduledDepartureLocalDateTime { get; set; }
        public LocalDateTime ScheduledDepartureUtcDateTime { get; set; }
        public LocalDateTime ScheduledArrivalLocalDateTime { get; set; }
        public LocalDateTime ScheduledArrivalUtcDateTime { get; set; }
        public AirportAndTerminal Origin { get; set; }
        public AirportAndTerminal Destination { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
    }
}
