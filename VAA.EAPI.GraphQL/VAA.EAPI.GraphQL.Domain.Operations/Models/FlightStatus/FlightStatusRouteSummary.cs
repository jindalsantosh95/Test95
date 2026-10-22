#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusRouteSummary
{
    public string? DefaultRouteResultsStopsIndicator { get; init; }
    public bool? MultiStop { get; init; }
    public bool? NonStop { get; init; }
    public bool? OneStop { get; init; }
    public bool? TimeRangeOne { get; init; }
    public bool? TimeRangeTwo { get; init; }
    public bool? TimeRangeThree { get; init; }
}
