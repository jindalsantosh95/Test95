namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class SliceFare
    {
        public string? Availability { get; set; }
        public string? Id { get; set; }
        public string? FareId { get; set; }
        public SliceFareContent? Content { get; set; }
        public PriceWithTax? Price { get; set; }
        public IEnumerable<SliceFareSegment>? FareSegments { get; set; }
        public int? Available { get; set; }
        public string FareFamilyType { get; set; }
        public bool CabinSelected { get; set; }
        public string? PromoCodeApplied { get; set; }
        public bool? IsSaverFare { get; set; }
        public IEnumerable<Link> Links { get; set; }
        public int? AvailableSeatCount { get; set; }
    }
}