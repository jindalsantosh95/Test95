using NodaTime;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaTrm;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class Segment
    {
        public string SegmentId { get; set; }
        public int Ordinal { get; set; }
        public FlightCarrier OperatingCarrier { get; set; }
        public FlightCarrier MarketingCarrier { get; set; }
        public LocalDateTime? ScheduledDepartureLocalDateTime { get; set; }
        public LocalDateTime? ScheduledDepartureUtcDateTime { get; set; }
        public LocalDateTime? ScheduledArrivalLocalDateTime { get; set; }
        public LocalDateTime? ScheduledArrivalUtcDateTime { get; set; }
        public AirportAndTerminal Origin { get; set; }
        public AirportAndTerminal Destination { get; set; }
        public IEnumerable<Leg> Legs { get; set; }

        public static explicit operator Segment((IGrouping<string, DeltaSsci.Segment> ssciSegment, DeltaTrm.DatedOperatingSegment trmSegment) source)
        {
            var segment = (Segment)source.trmSegment;
            segment.Legs = source.trmSegment.Legs.Select(l => (Leg)(source.ssciSegment.First(s => s.OriginCode == l.Departure.Station.Code), l));
            segment.Origin = (AirportAndTerminal)(source.trmSegment.Legs.First().Departure.Station, source.ssciSegment.First().Terminal);

            return segment;

        }

        public static explicit operator Segment(DeltaTrm.DatedOperatingSegment trmSegment)
        {
            var departure = trmSegment.Legs.First().Departure;
            var arrival = trmSegment.Legs.Last().Arrival;
            var marketingCarrier = trmSegment.MarketingSegment.MarketingCarrier.FirstOrDefault();

            return new Segment()
            {
                SegmentId = trmSegment.Id,
                Ordinal = trmSegment.Ordinal,
                OperatingCarrier = (FlightCarrier)(trmSegment.OperatingCarrier, trmSegment.FlightNum),
                MarketingCarrier = (FlightCarrier)(marketingCarrier, trmSegment.MarketingSegment.FlightNum),
                ScheduledDepartureLocalDateTime = DateTimeHelper.ParseLocalIsoDateTime(departure.ScheduledDepartureLocalDateTime),
                ScheduledDepartureUtcDateTime = DateTimeHelper.ParseLocalIsoDateTime(departure.ScheduledDepartureUTCDateTime),
                ScheduledArrivalLocalDateTime = DateTimeHelper.ParseLocalIsoDateTime(arrival.ScheduledArrivalLocalDateTime),
                ScheduledArrivalUtcDateTime = DateTimeHelper.ParseLocalIsoDateTime(arrival.ScheduledArrivalUTCDateTime),
                Origin = (AirportAndTerminal)(departure.Station, null),
                Destination = (AirportAndTerminal)(arrival.Station, null),
                Legs = trmSegment.Legs.Select(l => (Leg)l)
            };
        }
    }
}