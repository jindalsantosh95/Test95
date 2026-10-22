using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryCarryOnBag")]
    public class CarryOnBag
    {
        public PassengerBaggageAllowance? PassengerBaggageAllowance { get; set; }
    }
}
