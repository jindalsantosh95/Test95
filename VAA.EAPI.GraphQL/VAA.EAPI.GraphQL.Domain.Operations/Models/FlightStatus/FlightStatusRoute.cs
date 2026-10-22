#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusRoute
{
    public FlightStatusAirport? DepartureAirport { get; init; }
    public FlightStatusAirport? ArrivalAirport { get; init; }
    public string? DepartureDate { get; init; }
    public IEnumerable<FlightStatusRouteOption?>? Routes { get; init; }
    public FlightStatusRouteSummary? RouteSummary { get; init; }
}
