namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class Legs
    {
        public ScheduledDepartureTime ScheduledDepartureTime { get; set; }
        public string LegSequenceNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string OperatingCarrier { get; set; }
        public ScheduledArrivalTime ScheduledArrivalTime { get; set; }
        public string DestinationAirportCode { get; set; }
        public string LegDuration { get; set; }
        public LegDistance LegDistance { get; set; }
    }
}
