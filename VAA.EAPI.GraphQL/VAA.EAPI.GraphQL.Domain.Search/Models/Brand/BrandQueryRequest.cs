namespace VAA.EAPI.GraphQL.Domain.Search.Models.Brand
{
    public class BrandQueryRequest
    {
        public string? Currency { get; set; }
        public IEnumerable<BrandQueryRequestTrip>? Itinerary { get; set; }
        public string? PseudoCityCode { get; set; }
        public string? AccountCode { get; set; }
    }
}