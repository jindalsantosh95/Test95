using HotChocolate;
using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:Segment")]
    public class Segment
    {
        public string SegmentId { get; set; }

        [GraphQLDescription("Segment order inside the slice.")]
        public int Ordinal { get; set; }
        public FlightCarrier OperatingCarrier { get; set; }
        public FlightCarrier MarketingCarrier { get; set; }
        public LocalDateTime ScheduledDepartureLocalDateTime { get; set; }
        public LocalDateTime ScheduledDepartureUtcDateTime { get; set; }
        public LocalDateTime ScheduledArrivalLocalDateTime { get; set; }
        public LocalDateTime ScheduledArrivalUtcDateTime { get; set; }

        [GraphQLDescription("Segment origin airport and terminal.")]
        public AirportAndTerminal Origin { get; set; }

        [GraphQLDescription("Segment destination airport and terminal.")]
        public AirportAndTerminal Destination { get; set; }

        [GraphQLDescription("Legs in the segment. Most segments contain one leg, but on some routes an aircraft may land with some or all passengers remaining on board when the aircraft continues to its next destination. On Virgin Atlantic we do this to serve two Caribbean islands from a single UK departure.")]
        public IEnumerable<Leg> Legs { get; set; }

        public static explicit operator Segment(Infrastructure.Models.CheckInMS.Segment source) => new()
        {
            SegmentId = source.SegmentId,
            Ordinal = source.Ordinal,
            OperatingCarrier = source.OperatingCarrier != null ? (FlightCarrier)source.OperatingCarrier : null,
            MarketingCarrier = source.MarketingCarrier != null ? (FlightCarrier)source.MarketingCarrier : null,
            ScheduledDepartureLocalDateTime = source.ScheduledDepartureLocalDateTime,
            ScheduledDepartureUtcDateTime = source.ScheduledDepartureUtcDateTime,
            ScheduledArrivalLocalDateTime = source.ScheduledArrivalLocalDateTime,
            ScheduledArrivalUtcDateTime = source.ScheduledArrivalUtcDateTime,
            Origin = source.Origin != null ? (AirportAndTerminal)source.Origin : null,
            Destination = source.Destination != null ? (AirportAndTerminal)source.Destination : null,
            Legs = source.Legs?.Select(l => (Leg)l)
        };
    }
}
