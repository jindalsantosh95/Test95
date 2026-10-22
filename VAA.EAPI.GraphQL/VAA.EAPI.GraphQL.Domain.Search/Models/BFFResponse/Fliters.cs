namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class FilterCollection
    {
        public IEnumerable<Filter> Filters { get; set; }
        public int Total { get; set; }
        public int Count { get; set; }
        public IEnumerable<Link> Links { get; set; } = Enumerable.Empty<Link>();
    }
}
