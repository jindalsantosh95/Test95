using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS
{
    [GraphQLName("TripsBasket")]
    public class Basket
    {
        public Guid BasketId { get; set; }
        public ContactDetails ContactDetails { get; set; }
        public Booking? BookingDetails { get; set; }
        public IEnumerable<FlightPassengerSeatMap> FlightPassengerSeatMap { get; set; }
        public IEnumerable<PaymentSession> PaymentSessions { get; set; }
        public BasketStatus Status { get; set; }
    }
}