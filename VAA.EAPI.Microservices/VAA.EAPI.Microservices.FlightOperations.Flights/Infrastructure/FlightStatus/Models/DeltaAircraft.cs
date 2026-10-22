namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaAircraft
    {
        public string? Type { get; set; }
        public string? DeltaAircraftTypeCode { get; set; }
        public string? IndustryStandardAircraftTypeCode { get; set; }
        public string? Error { get; set; }
        public DeltaAircraftSpecification? Specification { get; set; }
    }
}
