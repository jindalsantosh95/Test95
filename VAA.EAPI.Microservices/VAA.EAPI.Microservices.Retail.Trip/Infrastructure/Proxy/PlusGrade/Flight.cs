namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.PlusGrade
{
    public class Flight
    {
        public string CarrierCode { get; set; }
        public string FlightNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string DepartureDate { get; set; }
        public string FareClass { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string Interline { get; set; }
        public string WebCheckInPeriodOpen { get; set; }
    }
}

