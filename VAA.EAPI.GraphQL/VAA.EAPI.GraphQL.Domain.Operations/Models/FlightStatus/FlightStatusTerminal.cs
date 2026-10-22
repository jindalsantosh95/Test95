#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusTerminal
{
    public bool? DefaultTerminal { get; init; }
    public FlightStatusGate? Gate { get; init; }
    public string? Id { get; init; }
    public string? Name { get; init; }
}
