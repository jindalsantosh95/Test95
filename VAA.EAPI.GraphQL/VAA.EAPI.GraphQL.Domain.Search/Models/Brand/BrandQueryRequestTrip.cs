namespace VAA.EAPI.GraphQL.Domain.Search.Models.Brand
{
    public class BrandQueryRequestTrip
    {
        public string? PointOfSale { get; set; }
        public string? Source { get; set; }
        public IEnumerable<BrandQueryRequestSegment>? Segments { get; set; }
        public IEnumerable<string>? PassengerTypeCodes { get; set; }
    }
}