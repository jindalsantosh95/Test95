#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusAircraftEngineDescription
{
    public string? EngineDescription1 { get; init; }
    public string? EngineDescription2 { get; init; }

    public static explicit operator FlightStatusAircraftEngineDescription(FlightStatusInternalAircraftEngineDescription source) => new()
    {
        EngineDescription1 = source.EngineDescription1,
        EngineDescription2 = source.EngineDescription2
    };
}
