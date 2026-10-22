#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusFlightState
{
    public string? Description { get; init; }
    public string? Color { get; init; }
    public string? ScheduledDepartureTime { get; init; }
    public string? ScheduledArrivalTime { get; init; }
    public string? EstimatedDepartureTime { get; init; }
    public string? EstimatedArrivalTime { get; init; }
    public string? ActualDepartureTime { get; init; }
    public string? ActualArrivalTime { get; init; }

    public static explicit operator FlightStatusFlightState(FlightStatusInternalState source) =>
        FromInternal(source, null, null)!;

    internal static FlightStatusFlightState? FromInternal(
        FlightStatusInternalState? source,
        FlightStatusFlightPoint? departure,
        FlightStatusFlightPoint? arrival) => source is null
            ? null
            : new FlightStatusFlightState
            {
                Description = source.Description,
                Color = source.Color,
                ScheduledDepartureTime = departure?.ScheduledTime,
                ScheduledArrivalTime = arrival?.ScheduledTime,
                EstimatedDepartureTime = departure?.EstimatedTime,
                EstimatedArrivalTime = arrival?.EstimatedTime,
                ActualDepartureTime = departure?.ActualTime,
                ActualArrivalTime = arrival?.ActualTime
            };
}
