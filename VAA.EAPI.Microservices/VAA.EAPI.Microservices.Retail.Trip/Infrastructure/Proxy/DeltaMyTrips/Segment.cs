namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class Segment
    {
        public int SegmentId { get; set; }
        public string FlightSegmentId { get; set; }
        public IEnumerable<OperatingCarrier> OperatingCarrier { get; set; }
        public string FlightNum { get; set; }
        public MarketingSegment MarketingSegment { get; set; }
        public OriginStation OriginStation { get; set; }
        public DestinationStation DestinationStation { get; set; }  
        public bool CheckedIn { get; set; }
        public string ActionCode { get; set; }
        public bool RedEye { get; set; }
        public bool Misconnect { get; set; }
        public IEnumerable<Leg> Legs { get; set; }
        public bool ChangeOfGauge { get; set; }
        public int? Ordinal { get; set; }
        public bool? ThroughFlight { get; set; }
        public OperationalStatus? OperationalStatus { get; set; }
    }
}