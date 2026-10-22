using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryLink")]
    public class Link
    {
        public string? Rel { get; set; }
        public string? Href { get; set; }
    }
}