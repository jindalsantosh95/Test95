namespace VAA.EAPI.GraphQL.Domain.Search.Models.Holiday
{
    public class Facet
    {
        public IEnumerable<string> LocationId { get; set; } = Enumerable.Empty<string>();
        public string? Description { get; set; }
        public IEnumerable<Facet> Children { get; set; } = Enumerable.Empty<Facet>();
    }
}
