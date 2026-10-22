#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusInternalState
{
    public string? Description { get; init; }
    public string? Color { get; init; }
    public string? Message { get; init; }
}
