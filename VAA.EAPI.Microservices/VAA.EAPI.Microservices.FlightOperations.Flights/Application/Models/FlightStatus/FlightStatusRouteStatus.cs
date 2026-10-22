using Newtonsoft.Json;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusRouteStatus
    {
        public static explicit operator FlightStatusRouteStatus?(DeltaStatus? source) => source is null
            ? null
            : new FlightStatusRouteStatus
            {
                StatusColor = source.StatusColor,
                StatusDescription = source.StatusDescription,
                ScheduledDepartureTime = source.ScheduledDepartureTime,
                ScheduledArrivalTime = source.ScheduledArrivalTime,
                ScheduledGmtDepartureTime = source.ScheduledGMTDepartureTime,
                ScheduledGmtArrivalTime = source.ScheduledGMTArrivalTime,
                ActualDepartureTime = source.ActualDepartureTime,
                ActualArrivalTime = source.ActualArrivalTime,
                ActualGmtDepartureTime = source.ActualGMTDepartureTime,
                ActualGmtArrivalTime = source.ActualGMTArrivalTime,
                ActualOutGmtTime = source.ActualOutGMTTime,
                ActualOutTime = source.ActualOutTime,
                ArrivalStateCode = source.ArrivalStateCode,
                EstimatedDepartureTime = source.EstimatedDepartureTime,
                EstimatedArrivalTime = source.EstimatedArrivalTime,
                EstimatedGmtDepartureTime = source.EstimatedGMTDepartureTime,
                EstimatedGmtArrivalTime = source.EstimatedGMTArrivalTime,
                FlightDuration = source.FlightDuration,
                FlightDurationFullText = source.FlightDurationFullText,
                FlightTime = source.FlightTime,
                FlightTimeFullText = source.FlightTimeFullText,
                RemainingTime = source.RemainingTime,
                RemainingTimeFullText = source.RemainingTimeFullText,
                NotificationAvailability = source.NotificationAvailability,
                FutureDayDeparture = source.FutureDayDeparture,
                FutureDayDepartureDate = source.FutureDayDepartureDate,
                FutureDayArrival = source.FutureDayArrival,
                FutureDayArrivalDate = source.FutureDayArrivalDate,
                BaggageArrival = (FlightStatusRouteBaggageArrival?)source.BaggageArrival,
                CancelReasonMessage = source.CancelReasonMessage,
                CancelReasonCode = source.CancelReasonCode,
                DelayReasonMessage = source.DelayReasonMessage,
                DelayReasonCode = source.DelayReasonCode,
                NowAvailable = source.NowAvailable,
                NowDepartureTime = source.NowDepartureTime,
                NowDepartureDate = source.NowDepartureDate,
                NowArrivalTime = source.NowArrivalTime,
                NowArrivalDate = source.NowArrivalDate
            };

        public string? StatusColor { get; set; }
        public string? StatusDescription { get; set; }
        public string? ScheduledDepartureTime { get; set; }
        public string? ScheduledArrivalTime { get; set; }
        [JsonProperty("scheduledGMTDepartureTime")]
        public string? ScheduledGmtDepartureTime { get; set; }
        [JsonProperty("scheduledGMTArrivalTime")]
        public string? ScheduledGmtArrivalTime { get; set; }
        public string? ActualDepartureTime { get; set; }
        public string? ActualArrivalTime { get; set; }
        [JsonProperty("actualGMTDepartureTime")]
        public string? ActualGmtDepartureTime { get; set; }
        [JsonProperty("actualGMTArrivalTime")]
        public string? ActualGmtArrivalTime { get; set; }
        [JsonProperty("actualOutGMTTime")]
        public string? ActualOutGmtTime { get; set; }
        public string? ActualOutTime { get; set; }
        public string? ArrivalStateCode { get; set; }
        public string? EstimatedDepartureTime { get; set; }
        public string? EstimatedArrivalTime { get; set; }
        [JsonProperty("estimatedGMTDepartureTime")]
        public string? EstimatedGmtDepartureTime { get; set; }
        [JsonProperty("estimatedGMTArrivalTime")]
        public string? EstimatedGmtArrivalTime { get; set; }
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
        public FlightStatusRouteBaggageArrival? BaggageArrival { get; set; }
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
