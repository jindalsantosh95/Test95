namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaEncryptedBookingDetails
{
    public class Trip
    {
        public string TripId { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
    }
}