namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    [GraphQLName("Link_BFF")]
    public class Link
    {
        public string? Rel { get; set; }
        public string? Href { get; set; }
    }
}