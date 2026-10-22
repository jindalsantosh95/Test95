namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaRouteSearchResponse
    {
        public string? DepartureDate { get; set; }
        public DeltaAirport? DepartureAirport { get; set; }
        public DeltaAirport? ArrivalAirport { get; set; }
        public IEnumerable<DeltaRoute>? Routes { get; set; }
        public DeltaRouteSummary? RouteSummary { get; set; }
    }
}
