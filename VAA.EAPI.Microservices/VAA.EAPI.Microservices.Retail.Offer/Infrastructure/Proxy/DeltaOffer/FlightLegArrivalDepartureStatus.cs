namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class FlightLegArrivalDepartureStatus
    {
        public string ActualArrivalLocalTs { get; set; }
        public string ActualArrivalUtcTs { get; set; }
        public string ActualDepartureLocalTs { get; set; }
        public string ActualDepartureUtcTs { get; set; }
        public string ActualOutLocalTs { get; set; }
        public string ActualOutUtcTs { get; set; }
        public string ArrivalStateCode { get; set; }
        public string EstimatedArrivalLocalTs { get; set; }
        public string EstimatedArrivalUtcTs { get; set; }
        public string EstimatedDepartureLocalTs { get; set; }
        public string EstimatedDepartureUtcTs { get; set; }
        public string InFlightDurationTime { get; set; }
        public string NotificationAvailabilityText { get; set; }
        public string RemainingInFlightDurationTime { get; set; }
        public string ScheduledArrivalLocalTs { get; set; }
        public string ScheduledArrivalUtcTs { get; set; }
        public string ScheduledDepartureLocalTs { get; set; }
        public string ScheduledDepartureUtcTs { get; set; }
        public string ScheduledFlightLegDurationTime { get; set; }
        public string StatusColorCode { get; set; }
        public string StatusDesc { get; set; }
    }
}
