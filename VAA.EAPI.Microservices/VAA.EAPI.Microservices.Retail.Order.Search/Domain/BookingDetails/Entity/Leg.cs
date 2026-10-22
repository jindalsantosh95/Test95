namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity
{
    public class Leg
    {
        public string LegSequenceNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public ScheduledDepartureTime ScheduledDepartureTime { get; set; }
        public ScheduledArrivalTime ScheduledArrivalTime { get; set; }
        public string EquipmentType { get; set; }
        public LegDistance LegDistance { get; set; }
        public string LegDuration { get; set; }
        public string DelegatedCarrier { get; set; }
        public string OperatingCarrier { get; set; }
    }
}
