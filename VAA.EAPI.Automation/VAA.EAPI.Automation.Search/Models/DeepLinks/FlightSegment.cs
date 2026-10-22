namespace VAA.EAPI.Automation.Search.Models.DeepLinks
{
    public class FlightSegment
    {
        public int sliceNumber { get; set; }
        public int segmentId { get; set; }
        public string marketingCarrierCode { get; set; }
        public string marketingFlightNumber { get; set; }
        public string originAirportCode { get; set; }
        public string destinationAirportCode { get; set; }
        public string marketingClassOfService { get; set; }
        public string scheduledDepartureLocalDateTime { get; set; }
        public string fareBasisCode { get; set; }
    }
}