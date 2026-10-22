namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class SpecialServiceRequest
    {
        public string AirlineCode { get; set; }
        public string AllPassengersFlag { get; set; }
        public string AllSegmentsFlag { get; set; }
        public string FirstNameNumber { get; set; }
        public string FlightDate { get; set; }
        public string FlightNumber { get; set; }
        public string IsHAFACT { get; set; }
        public string LastNameNumber { get; set; }
        public string NumberOfSsrsRequestedFor { get; set; }
        public string OaSpecialServiceRequestFlag { get; set; }
        public string RecordName { get; set; }
        public string Remarks { get; set; }
        public string RequestCode { get; set; }
        public string SegmentNumber { get; set; }
        public string SsrNumber { get; set; }
        public string StatusCode { get; set; }
    }
}
