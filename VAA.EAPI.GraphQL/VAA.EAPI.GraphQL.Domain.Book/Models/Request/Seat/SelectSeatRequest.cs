namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Seat
{
    public class SelectSeatRequest
    {
        public string BasketId { get; set; }
        public FlightPassengerSeatMap FlightPassengerSeatMap { get; set; }
    }
}