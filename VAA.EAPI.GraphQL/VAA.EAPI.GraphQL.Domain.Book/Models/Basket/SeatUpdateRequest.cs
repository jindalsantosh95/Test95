using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Seat;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    public class SeatUpdateRequest
    {
        public string BasketId { get; set; }
        public IEnumerable<FlightPassengerSeatMap> FlightPassengerSeatMap { get; set; }
    }
}