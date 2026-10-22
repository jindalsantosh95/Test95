#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusFlightPoint
{
    public string? AirportCode { get; init; }
    public string? Terminal { get; init; }
    public string? Gate { get; init; }
    public string? ScheduledTime { get; init; }
    public string? EstimatedTime { get; init; }
    public string? ActualTime { get; init; }

    public static explicit operator FlightStatusFlightPoint(FlightStatusInternalPoint source) => new()
    {
        AirportCode = source.AirportCode,
        Terminal = source.Terminal,
        Gate = source.Gate,
        ScheduledTime = source.ScheduledTime,
        EstimatedTime = source.EstimatedTime,
        ActualTime = source.ActualTime
    };
}
