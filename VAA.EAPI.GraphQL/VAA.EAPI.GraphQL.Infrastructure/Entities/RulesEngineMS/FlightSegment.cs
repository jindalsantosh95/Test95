namespace VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS
{
    public class FlightSegment
    {
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public int SegmentDistance { get; set; }
        public string OperatingCarrier { get; set; }
        public string MarketingCarrier { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string ScheduledDepartureTime { get; set; }
        public string CabinClass { get; set; }
        public string ClassOfService { get; set; }
        public string OperatingBookingClass { get; set; }
        public string MarketingBookingClass { get; set; }        
        public string SalesLocation { get; set; }
        public string AgentIataNumber { get; set; }
        public string OriginCountryCode { get; set; }
        public string DestinationCountryCode { get; set; }
        public string FareBasisCode { get; set; }
        public string FlownClass { get; set; }
    }
}
