using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryFeature")]
    public class Feature
    {
        public string? Type { get; set; }
        public string? Description { get; set; }
    }
}