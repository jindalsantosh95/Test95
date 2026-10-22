namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.SCVResults
{
    public class Leg
    {
        public string DelegatedCarrier { get; set; }
        public string Destination_Airport_Code { get; set; }
        public string EquipmentType { get; set; }
        public LegDistance LegDistance { get; set; }
        public string LegDuration { get; set; }
        public string LegSequenceNumber { get; set; }
        public string OperatingCarrier { get; set; }
        public string OriginAirportCode { get; set; }
        public ScheduledArrivalTime ScheduledArrivalTime { get; set; }
        public ScheduledDepartureTime ScheduledDepartureTime { get; set; }
    }
}
