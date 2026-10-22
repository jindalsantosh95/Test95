using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket
{
    public class Slice
    {
        public string Id { get; set; }
        public LocalDateTime? ScheduledDepartureLocalDateTime { get; set; }
        public LocalDateTime? ScheduledArrivalLocalDateTime { get; set; }
        public Infrastructure.Models.CheckInMS.AirportAndTerminal Origin { get; set; }
        public Infrastructure.Models.CheckInMS.AirportAndTerminal Destination { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
    }
}