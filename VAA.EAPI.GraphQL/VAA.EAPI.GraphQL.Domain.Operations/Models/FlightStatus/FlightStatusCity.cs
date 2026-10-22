#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusCity
{
    public string? Name { get; init; }
    public string? Region { get; init; }
    public string? RegionFullText { get; init; }
}
