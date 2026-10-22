namespace VAA.EAPI.GraphQL.Domain.Book.Models.InRuleEngine
{
    public class EarnCalculatorFlightSegment
    {
        public string OriginAirportCode { get; set; }

        public string DestinationAirportCode { get; set; }

        public int SegmentDistance { get; set; }

        public string OperatingCarrier { get; set; }

        public string MarketingCarrier { get; set; }

        public string OperatingFlightNumber { get; set; }

        public string MarketingFlightNumber { get; set; }

        public string ScheduledDepartureTime { get; set; }

        public string ClassOfService { get; set; }
        public string OperatingBookingClass { get; set; }
        public string MarketingBookingClass { get; set; }
        public string CabinClass { get; set; }
    }
}