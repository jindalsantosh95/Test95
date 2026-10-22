#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusAircraftSpeed
{
    public int? InMph { get; init; }
    public int? InKmph { get; init; }

    public static explicit operator FlightStatusAircraftSpeed(FlightStatusInternalAircraftSpeed source) => new()
    {
        InMph = source.InMph,
        InKmph = source.InKmph
    };
}
