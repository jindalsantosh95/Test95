namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class Seat
    {
        public string? Column { get; set; }
        public string? Number { get; set; }
        public SeatRow? SeatRow { get; set; }
        public IEnumerable<ActualCabinClass>? ActualCabinClass { get; set; }
    }
}
