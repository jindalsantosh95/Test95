namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaFlightDetailsResponse
    {
        public string? SelectedLegId { get; set; }
        public string? FlightDate { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public DeltaSegment? Segment { get; set; }
    }
}
