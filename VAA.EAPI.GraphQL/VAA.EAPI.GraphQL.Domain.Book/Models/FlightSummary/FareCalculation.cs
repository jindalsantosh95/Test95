using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryFareCalculation")]
    public class FareCalculation
    {
        public string? PassengerType { get; set; }
        public Price? Fare { get; set; }
        public IEnumerable<Taxes>? Taxes { get; set; }
    }
}