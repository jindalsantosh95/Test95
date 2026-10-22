namespace VAA.EAPI.GraphQL.Domain.Search.Models.Sustain
{
    public sealed class SustainMSResponse
    {

        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string FlightNumber { get; set; }
        public string FlightDate { get; set; }
        public string RequestId { get; set; }
        public string SourceMethodology { get; set; }
        public string SourceQuality { get; set; }
        public decimal KilosCo2 { get; set; }
        public decimal KilosCo2e { get; set; }
        public decimal LbsCo2 { get; set; }
        public decimal LbsCo2e { get; set; }
        public string Source { get; set; }
        public string StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string Cabin { get; set; }
    }
}
