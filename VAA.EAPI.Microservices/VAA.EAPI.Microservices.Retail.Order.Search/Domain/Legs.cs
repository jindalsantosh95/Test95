namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class Legs
    {
        public ScheduledDepartureTime ScheduledDepartureTime { get; set; }
        public string LegSequenceNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string OperatingCarrier { get; set; }
        public ScheduledArrivalTime ScheduledArrivalTime { get; set; }
        public string Destination_Airport_Code { get; set; }
        public string LegDuration { get; set; }
        public string StopOverDuration { get; set; }
        public LegDistance LegDistance { get; set; }
        public string OriginTerminal { get; set; }
        public string DestinationTerminal { get; set; }
    }
}
