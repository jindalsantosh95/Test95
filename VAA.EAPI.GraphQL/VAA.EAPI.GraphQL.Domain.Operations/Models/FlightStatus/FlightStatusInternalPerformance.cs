#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusInternalPerformance
{
    public int? MilesFlown { get; init; }
    public bool? OnTime { get; init; }
    public bool? ArrivalDelay30 { get; init; }
    public bool? ArrivalDelay60 { get; init; }
    public bool? Cancellation { get; init; }
}
