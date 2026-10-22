namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class FlightSegment
    {
        public Aircraft aircraft { get; set; }
        public string aircraftTypeCode { get; set; }
        public bool dayChange { get; set; }
        public DestinationAirport destinationAirport { get; set; }
        public string destinationAirportCode { get; set; }
        public string destinationCityCode { get; set; }
        public bool dominantSegment { get; set; }
        public List<FlightLeg> flightLeg { get; set; }
        public string flightSegmentNum { get; set; }
        public MarketingCarrier marketingCarrier { get; set; }
        public string marketingFlightNum { get; set; }
        public OperatingCarrier operatingCarrier { get; set; }
        public string operatingFlightNum { get; set; }
        public OriginAirport originAirport { get; set; }
        public string originAirportCode { get; set; }
        public string originCityCode { get; set; }
        public bool pendingGovtApproval { get; set; }
        public string scheduledArrivalLocalTs { get; set; }
        public string scheduledDepartureLocalTs { get; set; }
        public int stopCnt { get; set; }
        public string ticketingTimeLimitUtcTs { get; set; }
        public TotalAirTime totalAirTime { get; set; }
        public string flightSegmentId { get; set; }
        public string marketingClassOfServiceCode { get; set; }
        public Layover layover { get; set; }
    }
}
