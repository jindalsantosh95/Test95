#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusAircraftRange
{
    public int? InMiles { get; init; }
    public int? InKm { get; init; }

    public static explicit operator FlightStatusAircraftRange(FlightStatusInternalAircraftRange source) => new()
    {
        InMiles = source.InMiles,
        InKm = source.InKm
    };
}
