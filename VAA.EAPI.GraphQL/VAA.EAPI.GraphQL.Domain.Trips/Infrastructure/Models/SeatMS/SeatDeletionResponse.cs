using HotChocolate;
using Error = VAA.EAPI.GraphQL.Common.Entities.Error;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatMSDeletionResponse")]
    public class SeatDeletionResponse
    {
        public string OrderNumber { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public IEnumerable<Error> Error { get; set; }
        public string OrderId { get; set; }
        public bool Deletion { get; set; }
    }
}