namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaRoute
    {
        public string? PointToPointStops { get; set; }
        public string? TotalRouteTime { get; set; }
        public string? TotalRouteTimeFullText { get; set; }
        public bool? DiversionExists { get; set; }
        public IEnumerable<DeltaSegment>? Segments { get; set; }
    }
}
