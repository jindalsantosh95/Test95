using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class Slice
    {
        public string? Id { get; set; }
        public int DistanceMiles { get; set; }
        public int Ordinal { get; set; }
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
        public IEnumerable<Segment>? Segments { get; set; }
        public string? CheckinStatus { get; set; }
        public string TotalSliceDuration { get; set; }
        public bool CheckInEligible { get; set; }
        public IropProtect IropProtect { get; set; }

        public static explicit operator Slice(PassengerTrip source)
        {
            return new Slice
            {
                Id = source.TripId.ToString(),
                CheckinStatus = source.CheckinStatus,
                IropProtect = source.IropProtect != null ? (IropProtect)source.IropProtect : null,
                Segments = source.PassengerSegments?.Select(segment => (Segment)segment)
            };
        }

        public static explicit operator Slice(Infrastructure.Proxy.DeltaMyTrips.Trip source)
        {
            if (source == null)
                return null;

            TimeSpan? duration = source.TransportDestination?.ScheduledArrivalUtcDateTime - source.TransportOrigin?.ScheduledDepartureUtcDateTime;

            return new Slice
            {
                Id = source.TripId.ToString(),
                DistanceMiles = source.DistanceMiles,
                Ordinal = source.Ordinal,
                ScheduledDepartureLocalDateTime = source.TransportOrigin?.ScheduledDepartureLocalDateTime,
                ScheduledDepartureUtcDateTime = source.TransportOrigin?.ScheduledDepartureUtcDateTime,
                ScheduledArrivalLocalDateTime = source.TransportDestination?.ScheduledArrivalLocalDateTime,
                ScheduledArrivalUtcDateTime = source.TransportDestination?.ScheduledArrivalUtcDateTime,
                EstimatedDepartureLocalDateTime = source.TransportOrigin?.EstimatedDepartureLocalDateTime,
                EstimatedDepartureUtcDateTime = source.TransportOrigin?.EstimatedDepartureUtcDateTime,
                EstimatedArrivalLocalDateTime = source.TransportDestination?.EstimatedArrivalLocalDateTime,
                EstimatedArrivalUtcDateTime = source.TransportDestination?.EstimatedArrivalUtcDateTime,
                ActualDepartureLocalDateTime = source.TransportOrigin?.ActualDepartureLocalDateTime,
                ActualDepartureUtcDateTime = source.TransportOrigin?.ActualDepartureUtcDateTime,
                ActualArrivalLocalDateTime = source.TransportDestination?.ActualArrivalLocalDateTime,
                ActualArrivalUtcDateTime = source.TransportDestination?.ActualArrivalUtcDateTime,
                Origin = (AirportDetails)source.TransportOrigin,
                Destination = (AirportDetails)source.TransportDestination,
                Segments = source.Segments?.Select(segment => (Segment)segment),
                TotalSliceDuration = $"PT{(int)duration?.TotalHours}H{duration?.Minutes}M",
                CheckInEligible = source.CheckinEligible ?? false,
                IropProtect = source.IropProtect != null ? (IropProtect)source.IropProtect : null
            };
        }
    }
}