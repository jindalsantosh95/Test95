namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class PassengerSegment
    {
        public int SegmentId { get; set; }
        public BookedCabinClass? BookedCabinClass { get; set; }
        public string? CheckinStatus { get; set; }
        public IEnumerable<PassengerLeg>? PassengerLegs { get; set; }
    }


}
