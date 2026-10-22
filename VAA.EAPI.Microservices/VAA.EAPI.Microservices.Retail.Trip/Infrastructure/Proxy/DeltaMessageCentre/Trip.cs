namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre
{
    public class Trip
    {
        public int? TripId { get; set; }
        public int? Ordinal { get; set; }
        public TransportOrigin TransportOrigin { get; set; }
        public TransportDestination TransportDestination { get; set; }
        public IEnumerable<DatedOperatingSegment> Segments { get; set; }
    }
}
