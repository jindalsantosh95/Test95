namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OrderData
{
    public class FlightBookingDetail
    {
        public string OrderID { get; set; }
        public string SegmentNumber { get; set; }
        public string BookingDate { get; set; }
        public string OperatingCarrier { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public string BookingClass { get; set; }
        public string ScheduledDepartureTime { get; set; }
        public string BookingReference { get; set; }
        public string MarketingCarrier { get; set; }
        public string MarketingFlightNumber { get; set; }
        public IEnumerable<CustomerDetail> CustomerDetails { get; set; }
    }
}
