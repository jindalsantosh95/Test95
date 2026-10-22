namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class OrderBookingResponse
    {
        public IEnumerable<FlightBookingDetails> FlightBookingsDetails { get; set; }
    }
}
