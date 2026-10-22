namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{
    public class Trip
    {
        public int? TripId { get; set; }
        public int? Ordinal { get; set; }
        public TransportOrigin TransportOrigin { get; set; }
        public TransportDestination TransportDestination { get; set; }
        public IEnumerable<DatedOperatingSegment> Segments { get; set; }

        public static explicit operator Trip(Infrastructure.Proxy.DeltaMessageCentre.Trip source)
        {
            if (source == null)
                return null;

            return new Trip
            {
                Ordinal = source.Ordinal,
                TripId = source.TripId,
                TransportOrigin = (TransportOrigin)source.TransportOrigin,
                TransportDestination = (TransportDestination)source.TransportDestination,
                Segments = source.Segments?.Select(segment => (DatedOperatingSegment)segment)
            };
        }
    }
}
