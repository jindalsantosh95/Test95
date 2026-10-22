#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusPerformance
{
    public int? MilesFlown { get; init; }
    public bool? OnTime { get; init; }
    public bool? ArrivalDelay30 { get; init; }
    public bool? ArrivalDelay60 { get; init; }
    public bool? Cancellation { get; init; }

    public static explicit operator FlightStatusPerformance(FlightStatusInternalPerformance source) => new()
    {
        MilesFlown = source.MilesFlown,
        OnTime = source.OnTime,
        ArrivalDelay30 = source.ArrivalDelay30,
        ArrivalDelay60 = source.ArrivalDelay60,
        Cancellation = source.Cancellation
    };
}
