using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryFareCalculationSummary")]
    public class FareCalculationSummary
    {
        public IEnumerable<Fare>? Fares { get; set; }
        public IEnumerable<Taxes>? Taxes { get; set; }
    }
}