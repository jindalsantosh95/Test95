namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models
{
    public sealed class CheckAircraftCategoryResponse
    {
        public IEnumerable<CheckAircraftCategoryResult> AircraftCategoryChecks { get; set; } = [];
    }
}
