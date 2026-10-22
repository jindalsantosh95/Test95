#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusInternalPoint
{
    public string? AirportCode { get; init; }
    public string? Terminal { get; init; }
    public string? Gate { get; init; }
    public string? ScheduledTime { get; init; }
    public string? EstimatedTime { get; init; }
    public string? ActualTime { get; init; }
}
