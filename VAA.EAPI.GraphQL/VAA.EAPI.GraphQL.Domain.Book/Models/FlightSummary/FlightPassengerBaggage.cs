using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryFlightPassengerBaggage")]
    public class FlightPassengerBaggage
    {
        public string? FlightRefId { get; set; }
        public CarryOnBag? CarryOnBag { get; set; }
        public CheckedBag? CheckedBag { get; set; }
    }
}
