namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class FlightLeg
    {
        public Aircraft aircraft { get; set; }
        public AircraftTypeSpecification aircraftTypeSpecification { get; set; }
        public string carrierCode { get; set; }
        public bool dayChange { get; set; }
        public DestinationAirport destinationAirport { get; set; }
        public string destinationAirportCode { get; set; }
        public Distance distance { get; set; }
        public bool domestic { get; set; }
        public bool dominantLeg { get; set; }
        public Duration duration { get; set; }
        public bool feeRestricted { get; set; }
        public Layover layover { get; set; }
        public string legId { get; set; }
        public MarketingCarrier marketingCarrier { get; set; }
        public bool operatedByOwnerCarrier { get; set; }
        public OperatingCarrier operatingCarrier { get; set; }
        public string operatingCarrierCode { get; set; }
        public OriginAirport originAirport { get; set; }
        public string originAirportCode { get; set; }
        public string originCityCode { get; set; }
        public bool redEye { get; set; }
        public string scheduledArrivalLocalTs { get; set; }
        public string scheduledDepartureLocalTs { get; set; }
        public string destinationCityCode { get; set; }
    }
}
