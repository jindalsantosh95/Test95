
namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.FutureFlights
{
    public class FlightBookingsDetail
    {
        public string OrderID { get; set; }
        public string OperatingCarrier { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public string BookingClass { get; set; }
        public string ScheduledDepartureTime { get; set; }
        public string BookingReference { get; set; }
        public string MarketingCarrier { get; set; }
        public string MarketingFlightNumber { get; set; }
    }
}
