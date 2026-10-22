using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryTaxes")]
    public class Taxes
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
        public Price? Price { get; set; }
    }
}