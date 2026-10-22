#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusAirline
{
    public string? Code { get; init; }
    public string? FlightNumber { get; init; }
    public string? Name { get; init; }
}
