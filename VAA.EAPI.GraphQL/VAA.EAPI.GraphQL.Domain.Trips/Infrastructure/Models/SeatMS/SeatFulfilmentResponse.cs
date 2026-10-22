using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatMSFulfilmentResponse")]
    public class SeatFulfilmentResponse
    {
        public string OrderNumber { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public IEnumerable<SeatError> Error { get; set; }
        public string OrderId { get; set; }
    }
}