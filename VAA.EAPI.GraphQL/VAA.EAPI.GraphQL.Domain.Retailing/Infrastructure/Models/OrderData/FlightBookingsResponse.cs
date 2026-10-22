using HotChocolate;
using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderData
{
    [GraphQLName("Retailing.FlightBookingsResponse")]
    public class FlightBookingsResponse: ErrorDetails
    {
        public IEnumerable<FlightBookingDetail> FlightBookingsDetails { get; set; }
    }
}
