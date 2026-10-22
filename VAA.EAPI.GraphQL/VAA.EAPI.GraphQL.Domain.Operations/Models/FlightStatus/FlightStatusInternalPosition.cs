#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusInternalPosition
{
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public int? AltitudeFeet { get; init; }
    public decimal? SpeedKnots { get; init; }
    public decimal? HeadingDegrees { get; init; }
    public string? LastUpdated { get; init; }
}
