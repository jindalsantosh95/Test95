#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusPosition
{
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public int? AltitudeFeet { get; init; }
    public decimal? SpeedKnots { get; init; }
    public decimal? HeadingDegrees { get; init; }
    public string? LastUpdated { get; init; }

    public static explicit operator FlightStatusPosition(FlightStatusInternalPosition source) => new()
    {
        Latitude = source.Latitude,
        Longitude = source.Longitude,
        AltitudeFeet = source.AltitudeFeet,
        SpeedKnots = source.SpeedKnots,
        HeadingDegrees = source.HeadingDegrees,
        LastUpdated = source.LastUpdated
    };
}
