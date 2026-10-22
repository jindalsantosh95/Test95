#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusLegStatus
{
    public string? StatusColor { get; init; }
    public string? StatusDescription { get; init; }
    public string? ScheduledDepartureTime { get; init; }
    public string? ScheduledArrivalTime { get; init; }
    public string? ScheduledGmtDepartureTime { get; init; }
    public string? ScheduledGmtArrivalTime { get; init; }
    public string? ActualDepartureTime { get; init; }
    public string? ActualArrivalTime { get; init; }
    public string? ActualGmtDepartureTime { get; init; }
    public string? ActualGmtArrivalTime { get; init; }
    public string? ActualOutGmtTime { get; init; }
    public string? ActualOutTime { get; init; }
    public string? ArrivalStateCode { get; init; }
    public string? EstimatedDepartureTime { get; init; }
    public string? EstimatedArrivalTime { get; init; }
    public string? EstimatedGmtDepartureTime { get; init; }
    public string? EstimatedGmtArrivalTime { get; init; }
    public string? FlightDuration { get; init; }
    public string? FlightDurationFullText { get; init; }
    public string? FlightTime { get; init; }
    public string? FlightTimeFullText { get; init; }
    public string? RemainingTime { get; init; }
    public string? RemainingTimeFullText { get; init; }
    public string? NotificationAvailability { get; init; }
    public bool? FutureDayDeparture { get; init; }
    public string? FutureDayDepartureDate { get; init; }
    public bool? FutureDayArrival { get; init; }
    public string? FutureDayArrivalDate { get; init; }
    public FlightStatusBaggageArrival? BaggageArrival { get; init; }
    public string? CancelReasonMessage { get; init; }
    public string? CancelReasonCode { get; init; }
    public string? DelayReasonMessage { get; init; }
    public string? DelayReasonCode { get; init; }
    public bool? NowAvailable { get; init; }
    public string? NowDepartureTime { get; init; }
    public string? NowDepartureDate { get; init; }
    public string? NowArrivalTime { get; init; }
    public string? NowArrivalDate { get; init; }
}
