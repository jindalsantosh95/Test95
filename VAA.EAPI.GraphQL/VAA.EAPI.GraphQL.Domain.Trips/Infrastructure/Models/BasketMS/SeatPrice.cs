using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS
{
    [GraphQLName("TripsBasketMSPassengerSeatPrice")]
    public class SeatPrice
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}