namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class BookingDetail
    {
        public string BookingReference { get; set; }
        public Booking Booking { get; set; }
        public string AuthorisationStatus { get; set; }
        public bool BookingLocked { get; set; }
    }
}

