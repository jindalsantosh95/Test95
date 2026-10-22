namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class FlightBookingDetails
    {
        public string OrderId { get; set; }
        public string OperatingCarrier { get; set; }
        public string MarketingCarrier { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public string BookingClass { get; set; }
        public string ScheduledArrivalTime { get; set; }
        public string ScheduledDepartureTime { get; set; }
        public string BookingReference { get; set; }
    }
}
