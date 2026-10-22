namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class Trip
    {
        public string destinationAirportCode { get; set; }
        public string destinationCityCode { get; set; }
        public bool differentDepartureAirport { get; set; }
        public List<FlightSegment> flightSegment { get; set; }
        public bool nearbyAirport { get; set; }
        public bool newSubFleetType { get; set; }
        public string originAirportCode { get; set; }
        public string originCityCode { get; set; }
        public string scheduledArrivalLocalTs { get; set; }
        public string scheduledDepartureLocalTs { get; set; }
        public int stopCnt { get; set; }
        public List<string> summarizedProductId { get; set; }
        public TotalTripTime totalTripTime { get; set; }
        public string tripId { get; set; }
        public List<string> flightSegmentIdList { get; set; }
        public List<TripBadge> tripBadges { get; set; }
    }
}
