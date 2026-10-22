#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusCoordinate
{
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
}
