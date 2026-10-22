namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaStatus
    {
        public string? StatusColor { get; set; }
        public string? StatusDescription { get; set; }
        public string? ScheduledDepartureTime { get; set; }
        public string? ScheduledArrivalTime { get; set; }
        public string? ScheduledGMTDepartureTime { get; set; }
        public string? ScheduledGMTArrivalTime { get; set; }
        public string? EstimatedDepartureTime { get; set; }
        public string? EstimatedArrivalTime { get; set; }
        public string? EstimatedGMTDepartureTime { get; set; }
        public string? EstimatedGMTArrivalTime { get; set; }
        public string? ActualDepartureTime { get; set; }
        public string? ActualArrivalTime { get; set; }
        public string? ActualGMTDepartureTime { get; set; }
        public string? ActualGMTArrivalTime { get; set; }
        public string? ActualOutGMTTime { get; set; }
        public string? ActualOutTime { get; set; }
        public string? ArrivalStateCode { get; set; }
        public string? FlightDuration { get; set; }
        public string? FlightDurationFullText { get; set; }
        public string? FlightTime { get; set; }
        public string? FlightTimeFullText { get; set; }
        public string? RemainingTime { get; set; }
        public string? RemainingTimeFullText { get; set; }
        public string? NotificationAvailability { get; set; }
        public bool? FutureDayDeparture { get; set; }
        public string? FutureDayDepartureDate { get; set; }
        public bool? FutureDayArrival { get; set; }
        public string? FutureDayArrivalDate { get; set; }
        public DeltaBaggageArrival? BaggageArrival { get; set; }
        public string? CancelReasonMessage { get; set; }
        public string? CancelReasonCode { get; set; }
        public string? DelayReasonMessage { get; set; }
        public string? DelayReasonCode { get; set; }
        public bool? NowAvailable { get; set; }
        public string? NowDepartureTime { get; set; }
        public string? NowDepartureDate { get; set; }
        public string? NowArrivalTime { get; set; }
        public string? NowArrivalDate { get; set; }
    }
}
