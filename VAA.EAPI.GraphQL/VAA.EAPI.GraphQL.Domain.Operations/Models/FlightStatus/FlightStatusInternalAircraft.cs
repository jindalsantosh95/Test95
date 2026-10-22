#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusInternalAircraft
{
    public string? Type { get; init; }
    public string? DeltaAircraftTypeCode { get; init; }
    public string? IndustryStandardAircraftTypeCode { get; init; }
    public string? Error { get; init; }
    public FlightStatusInternalAircraftSpecification? Specification { get; init; }
}
