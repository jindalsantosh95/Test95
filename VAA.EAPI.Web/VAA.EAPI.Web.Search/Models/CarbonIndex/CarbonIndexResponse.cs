namespace VAA.EAPI.Web.Search.Models.CarbonIndex
{
    public class CarbonIndexResponse
    {
        public string? OriginAirport { get; set; }
        public string? DestinationAirport { get; set; }
        public string? FlightNumber { get; set; }
        public string? FlightDate { get; set; }
        public string? RequestId { get; set; }
        public string? SourceMethodology { get; set; }
        public string? RefSourceMethodology { get; set; }
        public string? SourceQuality { get; set; }
        public string? RefSourceQuality { get; set; }
        public decimal KilosCo2e { get; set; }
        public decimal RefKilosCo2e { get; set; }
        public string? Source { get; set; }
        public string? RefSource { get; set; }
        public string? StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string[]? FareFamilies { get; set; }
    }
}
