using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryMetal")]
    public class Metal
    {
        public string? Family { get; set; }
        public string? Name { get; set; }
    }
}