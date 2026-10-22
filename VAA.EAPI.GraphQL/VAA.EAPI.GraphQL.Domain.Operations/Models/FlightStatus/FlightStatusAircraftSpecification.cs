#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusAircraftSpecification
{
    public FlightStatusAircraftImage? Image { get; init; }
    public FlightStatusAircraftRange? Range { get; init; }
    public FlightStatusAircraftSpeed? CruisingSpeed { get; init; }
    public int? SeatingCapacity { get; init; }
    public FlightStatusAircraftEngineDescription? EngineDescription { get; init; }

    public static explicit operator FlightStatusAircraftSpecification(FlightStatusInternalAircraftSpecification source) => new()
    {
        Image = source.Image is null ? null : (FlightStatusAircraftImage)source.Image,
        Range = source.Range is null ? null : (FlightStatusAircraftRange)source.Range,
        CruisingSpeed = source.CruisingSpeed is null ? null : (FlightStatusAircraftSpeed)source.CruisingSpeed,
        SeatingCapacity = source.SeatingCapacity,
        EngineDescription = source.EngineDescription is null
            ? null
            : (FlightStatusAircraftEngineDescription)source.EngineDescription
    };
}
