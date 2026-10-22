using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Seats
{
    [GraphQLName("TripsSeatPrice")]
    public class SeatPrice
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}