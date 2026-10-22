namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore
{
    public class Booking : HolidaysMS.Booking
    {
        public string Status { get; set; }
        public string Id { get; set; }
        public Pos Pos { get; set; }
        public IEnumerable<Product> Product { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public BookingResponseDetails ResponseStatus { get; set; }
        public TripDetails TripSearchRequest { get; set; }
        public bool? IsAccountLinkedPNR { get; set; }
        public IEnumerable<AdvisoryNote>? AdvisoryNotes { get; set; }

    }
}