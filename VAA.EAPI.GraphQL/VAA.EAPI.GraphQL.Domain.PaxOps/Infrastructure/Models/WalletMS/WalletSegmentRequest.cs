using NodaTime;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.WalletMS
{
    public class WalletSegmentRequest
    {
        public FlightCarrier OperatingCarrier { get; set; }
        public FlightCarrier MarketingCarrier { get; set; }
        public LocalDateTime? ScheduledDepartureLocalDateTime { get; set; }
        public LocalDateTime? ScheduledArrivalLocalDateTime { get; set; }
        public AirportAndTerminal Origin { get; set; }
        public AirportAndTerminal Destination { get; set; }
        public int Ordinal { get; set; }

        public static explicit operator WalletSegmentRequest(VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips.Segment segment)
        {
            return new WalletSegmentRequest
            {
                OperatingCarrier = (FlightCarrier)segment?.OperatingCarrier,
                MarketingCarrier = (FlightCarrier)segment?.MarketingCarrier,
                ScheduledDepartureLocalDateTime = segment == null ? null : LocalDateTime.FromDateTime(segment.ScheduledDepartureLocalDateTime),
                ScheduledArrivalLocalDateTime = segment == null ? null : LocalDateTime.FromDateTime(segment.ScheduledArrivalLocalDateTime),
                Origin = (AirportAndTerminal)segment?.Origin,
                Destination = (AirportAndTerminal)segment?.Destination,
                Ordinal = segment?.Ordinal ?? 0
            };
        }
    }
}
