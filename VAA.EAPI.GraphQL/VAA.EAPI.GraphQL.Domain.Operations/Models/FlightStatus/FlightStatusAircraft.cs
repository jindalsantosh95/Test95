#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusAircraft
{
    public string? Type { get; init; }
    public string? DeltaAircraftTypeCode { get; init; }
    public string? IndustryStandardAircraftTypeCode { get; init; }
    public FlightStatusAircraftSpecification? Specification { get; init; }

    public static explicit operator FlightStatusAircraft(FlightStatusInternalAircraft source) => new()
    {
        Type = source.Type,
        DeltaAircraftTypeCode = source.DeltaAircraftTypeCode,
        IndustryStandardAircraftTypeCode = source.IndustryStandardAircraftTypeCode,
        Specification = source.Specification is null
            ? null
            : (FlightStatusAircraftSpecification)source.Specification
    };
}
