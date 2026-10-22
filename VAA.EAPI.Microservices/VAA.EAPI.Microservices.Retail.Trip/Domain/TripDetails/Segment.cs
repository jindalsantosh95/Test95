using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;
using VAA.EAPI.Microservices.Retail.Trip.Mapper;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class Segment
    {
        public string? Id { get; set; }
        public int? Ordinal { get; set; }
        public CarrierDetails? OperatingCarrier { get; set; }
        public CarrierDetails? MarketingCarrier { get; set; }
        public DateTime? ScheduledDepartureLocalDateTime { get; set; }
        public DateTime? ScheduledDepartureUtcDateTime { get; set; }
        public DateTime? ScheduledArrivalLocalDateTime { get; set; }
        public DateTime? ScheduledArrivalUtcDateTime { get; set; }
        public DateTime? ScheduledArrivalBasedOnDepartureLocalDateTime { get; set; }
        public DateTime? EstimatedDepartureLocalDateTime { get; set; }
        public DateTime? EstimatedDepartureUtcDateTime { get; set; }
        public DateTime? EstimatedArrivalLocalDateTime { get; set; }
        public DateTime? EstimatedArrivalUtcDateTime { get; set; }
        public DateTime? ActualDepartureLocalDateTime { get; set; }
        public DateTime? ActualDepartureUtcDateTime { get; set; }
        public DateTime? ActualArrivalLocalDateTime { get; set; }
        public DateTime? ActualArrivalUtcDateTime { get; set; }
        public AirportDetails? Origin { get; set; }
        public AirportDetails? Destination { get; set; }
        public string? ActionCode { get; set; }
        public IEnumerable<Leg>? Legs { get; set; }
        public string? FlightSegmentId { get; set; }        
        public CabinClass? BookedCabinClass { get; set; }
        public string? CheckinStatus { get; set; }
        public string TotalSegmentDuration { get; set; }
        public string? FareBasisCode { get; set; }
        public OperationalStatus? OperationalStatus { get; set; }
        public bool CheckedIn { get; set; }

        public static explicit operator Segment(PassengerSegment source)
        {
            return new Segment
            {
                Id = source.SegmentId.ToString(),
                CheckinStatus = source.CheckinStatus,
                Legs = source.PassengerLegs?.Select(l => (Leg)l)
            };
        }

        public static explicit operator Segment(Infrastructure.Proxy.DeltaMyTrips.Segment source)
        {
            if (source == null)
                return null;

            TimeSpan? duration = source.Legs?.LastOrDefault()?.TransportDestination?.ScheduledArrivalUtcDateTime - source.Legs?.FirstOrDefault()?.TransportOrigin?.ScheduledDepartureUtcDateTime;

            return new Segment
            {
                Id = source.SegmentId.ToString(),
                Ordinal = source.Ordinal,
                ActionCode = source.ActionCode,
                FlightSegmentId = source.FlightSegmentId.ToString(),
                Origin = (AirportDetails)source.OriginStation,
                Destination = (AirportDetails)source.DestinationStation,
                Legs = source.Legs?.Select(l => (Leg)l),
                ScheduledArrivalBasedOnDepartureLocalDateTime = source.Legs?.LastOrDefault()?.TransportDestination?.ScheduledArrivalBasedOnDepartureLocalDateTime,
                ScheduledArrivalLocalDateTime = source.Legs?.LastOrDefault()?.TransportDestination?.ScheduledArrivalLocalDateTime,
                ScheduledDepartureLocalDateTime = source.Legs?.FirstOrDefault()?.TransportOrigin?.ScheduledDepartureLocalDateTime,
                ScheduledArrivalUtcDateTime = source.Legs?.LastOrDefault()?.TransportDestination?.ScheduledArrivalUtcDateTime,
                ScheduledDepartureUtcDateTime = source.Legs?.FirstOrDefault()?.TransportOrigin?.ScheduledDepartureUtcDateTime,
                EstimatedDepartureLocalDateTime = source.Legs?.FirstOrDefault()?.TransportOrigin?.EstimatedDepartureLocalDateTime,
                EstimatedDepartureUtcDateTime = source.Legs?.FirstOrDefault()?.TransportOrigin?.EstimatedDepartureUtcDateTime,
                EstimatedArrivalLocalDateTime = source.Legs?.LastOrDefault()?.TransportDestination?.EstimatedArrivalLocalDateTime,
                EstimatedArrivalUtcDateTime = source.Legs?.LastOrDefault()?.TransportDestination?.EstimatedArrivalUtcDateTime,
                ActualDepartureLocalDateTime = source.Legs?.FirstOrDefault()?.TransportOrigin?.ActualDepartureLocalDateTime,
                ActualDepartureUtcDateTime = source.Legs?.FirstOrDefault()?.TransportOrigin?.ActualDepartureUtcDateTime,
                ActualArrivalLocalDateTime = source.Legs?.LastOrDefault()?.TransportDestination?.ActualArrivalLocalDateTime,
                ActualArrivalUtcDateTime = source.Legs?.LastOrDefault()?.TransportDestination?.ActualArrivalUtcDateTime,
                MarketingCarrier = (CarrierDetails)source.MarketingSegment,
                OperatingCarrier = DeltaTripDetailsMapper.MapOperatingCarrier(source.OperatingCarrier?.FirstOrDefault(), source.FlightNum),
                TotalSegmentDuration = $"PT{(int)duration?.TotalHours}H{duration?.Minutes}M",
                FareBasisCode = source.MarketingSegment.FareBasisCode,
                OperationalStatus = source.OperationalStatus != null ? (OperationalStatus)source.OperationalStatus : null,
                CheckedIn = source.CheckedIn
            };
        }
    }
}