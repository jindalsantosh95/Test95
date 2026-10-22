namespace VAA.EAPI.Web.Search.Models.Search.Request
{
    public class FlightSegment
    {
        public int? SliceNumber { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public string MarketingClassOfService { get; set; }
        public string ScheduledDepartureLocalDateTime { get; set; }
        public string FareBasisCode { get; set; }
    }
}
