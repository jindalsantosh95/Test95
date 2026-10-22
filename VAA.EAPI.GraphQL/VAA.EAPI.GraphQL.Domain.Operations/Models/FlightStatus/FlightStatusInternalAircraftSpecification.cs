#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusInternalAircraftSpecification
{
    public FlightStatusInternalAircraftImage? Image { get; init; }
    public FlightStatusInternalAircraftRange? Range { get; init; }
    public FlightStatusInternalAircraftSpeed? CruisingSpeed { get; init; }
    public int? SeatingCapacity { get; init; }
    public FlightStatusInternalAircraftEngineDescription? EngineDescription { get; init; }
}
