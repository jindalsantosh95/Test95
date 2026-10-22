namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class TravelReservation
    {
        public string? ReservationResourceId { get; set; }
        public Reservation? Reservation { get; set; }
        public IEnumerable<Passenger>? Passengers { get; set; }
        public IEnumerable<Trip>? Trips { get; set; }
    }
}
