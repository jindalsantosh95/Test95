namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class FareDetails
    {
        public string? FareBasisCode { get; set; }
        public string? DominantFareProduct { get; set; }
        public IEnumerable<FareSegment>? FareSegments { get; set; }
        public string? FareCalcText { get; set; }
    }
}