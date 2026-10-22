using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryCheckedBag")]
    public class CheckedBag
    {
        public PassengerBaggageAllowance? PassengerBaggageAllowance { get; set; }
    }
}
