namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    internal static class DeltaFlightStatusModelExtensions
    {
        public static string? ToDisplayName(this DeltaTerminal? terminal) => terminal?.Name ?? terminal?.Id;
    }
}
