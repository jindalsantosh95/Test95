namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS
{
    public class Booking
    {
        public string Status { get; set; }
        public string Id { get; set; }
        public Pos Pos { get; set; }
        public string? ReservationType { get; set; }
        public IEnumerable<Product> Product { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public BookingResponseDetails ResponseStatus { get; set; }
        public TripDetails TripSearchRequest { get; set; }
        public bool? IsAccountLinkedPNR { get; set; }

    }
}