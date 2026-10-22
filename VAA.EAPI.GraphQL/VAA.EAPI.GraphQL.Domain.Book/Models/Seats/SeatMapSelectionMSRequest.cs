namespace VAA.EAPI.GraphQL.Domain.Book.Models.Seats
{
    public class SeatMapSelectionMSRequest
    {
        public string SeatOfferId { get; set; }
        public IEnumerable<PassengerSeatInfo> PassengerSeats { get; set; }
    }
}
