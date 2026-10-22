#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusInternalWarning
{
    public string? Code { get; init; }
    public string? Message { get; init; }
}
