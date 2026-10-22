using HotChocolate;
using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:Slice")]
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

        public static explicit operator Slice(Infrastructure.Models.CheckInMS.Slice source) => new()
        {
            SliceId = source.SliceId,
            ScheduledDepartureLocalDateTime = source.ScheduledDepartureLocalDateTime,
            ScheduledDepartureUtcDateTime = source.ScheduledDepartureUtcDateTime,
            ScheduledArrivalLocalDateTime = source.ScheduledArrivalLocalDateTime,
            ScheduledArrivalUtcDateTime = source.ScheduledArrivalUtcDateTime,
            Origin = source.Origin != null ? (AirportAndTerminal)source.Origin : null,
            Destination = source.Destination != null ? (AirportAndTerminal)source.Destination : null,
            Segments = source.Segments?.Select(s => (Segment)s)
        };
    }
}
