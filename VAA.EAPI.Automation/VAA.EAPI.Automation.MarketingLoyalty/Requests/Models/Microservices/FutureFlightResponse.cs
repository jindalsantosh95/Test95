namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.Microservices
{
    public class FutureFlightResponse
    {
        public string orderID { get; set; }
        public string operatingCarrier { get; set; }
        public string operatingFlightNumber { get; set; }
        public string originAirportCode { get; set; }
        public string destinationAirportCode { get; set; }
        public string bookingClass { get; set; }
        public string scheduledDepartureTime { get; set; }
        public string bookingReference { get; set; }
        public string marketingCarrier { get; set; }
        public string marketingFlightNumber { get; set; }
    }
}
