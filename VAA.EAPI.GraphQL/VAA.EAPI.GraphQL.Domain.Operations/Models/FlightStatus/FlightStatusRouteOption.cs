#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusRouteOption
{
    public string? PointToPointStops { get; init; }
    public string? TotalRouteTime { get; init; }
    public string? TotalRouteTimeFullText { get; init; }
    public bool? DiversionExists { get; init; }
    public IEnumerable<FlightStatusSegment?>? Segments { get; init; }
}
