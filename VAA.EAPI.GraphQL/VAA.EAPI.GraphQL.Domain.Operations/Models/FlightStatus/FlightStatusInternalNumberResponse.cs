#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusInternalNumberResponse
{
    public string? FlightDate { get; init; }
    public string? FlightNumber { get; init; }
    public string? SelectedLegId { get; init; }
    public IEnumerable<FlightStatusInternalLeg?>? Legs { get; init; }
    public FlightStatusInternalLeg? ActiveLeg { get; init; }
    public IEnumerable<FlightStatusInternalWarning?>? Warnings { get; init; }
}
