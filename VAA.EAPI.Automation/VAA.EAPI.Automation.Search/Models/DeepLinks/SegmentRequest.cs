namespace VAA.EAPI.Automation.Search.Models.DeepLinks
{
    public class SegmentRequest
    {
        public int sliceNumber { get; set; }
        public string marketingCarrierCode { get; set; }
        public string marketingFlightNumber { get; set; }
        public string originAirportCode { get; set; }
        public string destinationAirportCode { get; set; }
        public string marketingClassOfService { get; set; }
        public string scheduledDepartureLocalDateTime { get; set; }
        public string fareBasisCode { get; set; }
    }
}