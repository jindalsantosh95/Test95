namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class PassengerTrip
    {
        public int TripId { get; set; }
        public string CheckinStatus { get; set; }
        public IropProtect IropProtect { get; set; }
        public IEnumerable<PassengerSegment> PassengerSegments { get; set; }
    }
}
