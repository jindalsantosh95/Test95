namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public class Upgrade
    {
        public string? FareFamilyType { get; set; }
        public Price? Price { get; set; }
        public Content? Content { get; set; }
        public IEnumerable<Link>? Links { get; set; }
        public string? Id { get; set; }
        public string? FareId { get; set; }
        public string? PromoCodeApplied { get; set; }
    }
}