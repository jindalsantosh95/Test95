namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class Trip
    {
        public int TripId { get; set; }
        public int DistanceMiles { get; set; }
        public int Ordinal { get; set; }
        public TransportOrigin? TransportOrigin { get; set; }
        public TransportDestination? TransportDestination { get; set; }
        public IEnumerable<Segment>? Segments { get; set; }
        public bool International { get; set; }
        public bool? CheckinEligible { get; set; }
        public IropProtect? IropProtect { get; set; }
    }
}