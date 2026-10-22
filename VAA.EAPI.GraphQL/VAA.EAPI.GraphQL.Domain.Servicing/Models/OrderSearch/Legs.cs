namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class Legs
    {
        public ScheduledDepartureTime? ScheduledDepartureTime { get; set; }
        public string? LegSequenceNumber { get; set; }
        public string? OriginAirportCode { get; set; }
        public string? OperatingCarrier { get; set; }
        public ScheduledArrivalTime? ScheduledArrivalTime { get; set; }
        public string? Destination_Airport_Code { get; set; }
        public string? LegDuration { get; set; }
        public LegDistance? LegDistance { get; set; }
    }
}
