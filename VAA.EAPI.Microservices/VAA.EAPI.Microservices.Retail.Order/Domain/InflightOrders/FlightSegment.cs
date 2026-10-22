namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders
{
    public class FlightSegment
    {
        public string MarketingCarrier { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string MarketingFlightSuffix { get; set; }
        public string MarketingAirlineName { get; set; }
        public string OperatingCarrier { get; set; }
        public string OperatingFlightSuffix { get; set; }
        public string OperatingAirlineName { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string DepartureAirportCode { get; set; }
        public string DepartureAirportName { get; set; }
        public string DepartureCityCode { get; set; }
        public string DepartureCityName { get; set; }
        public string DepartureCountryCode { get; set; }
        public string DepartureCountryName { get; set; }
        public string ArrivalAirportCode { get; set; }
        public string ArrivalAirportName { get; set; }
        public string ArrivalCityCode { get; set; }
        public string ArrivalCityName { get; set; }
        public string ArrivalCountryCode { get; set; }
        public string ArrivalCountryName { get; set; }
        public LocalAndUTCDateTime ScheduledDepartureTime { get; set; }
        public LocalAndUTCDateTime ScheduledArrivalTime { get; set; }
    }
}
