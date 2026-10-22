#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusAirport
{
    public string? Code { get; init; }
    public FlightStatusTerminal? Terminal { get; init; }
    public string? Name { get; init; }
    public string? Url { get; init; }
    public string? CityName { get; init; }
    public FlightStatusCity? City { get; init; }
    public FlightStatusCoordinate? Coordinate { get; init; }
}
