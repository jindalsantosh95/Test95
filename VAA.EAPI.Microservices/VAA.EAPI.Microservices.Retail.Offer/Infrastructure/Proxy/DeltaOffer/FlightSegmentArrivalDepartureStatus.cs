namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class FlightSegmentArrivalDepartureStatus
    {
        public string ActualArrivalLocalTs { get; set; }
        public string ActualDepartureLocalTs { get; set; }
        public string ActualInLocalTime { get; set; }
        public string ActualOffLocalTime { get; set; }
        public string ActualOnLocalTime { get; set; }
        public string ActualOutLocalTime { get; set; }
        public string ArrivalStateCode { get; set; }
        public string EstimatedArrivalLocalTime { get; set; }
        public string EstimatedArrivalLocalTs { get; set; }
        public string EstimatedDepartureLocalTime { get; set; }
        public string EstimatedDepartureLocalTs { get; set; }
        public string FlightCanceledCode { get; set; }
        public string NotificationAvailabilityText { get; set; }
        public string ScheduledArrivalLocalTs { get; set; }
        public string ScheduledDepartureLocalTs { get; set; }
        public string ScheduledFlightSegmentDurationTime { get; set; }
        public string StatusColorCode { get; set; }
        public string StatusDesc { get; set; }
    }
}
