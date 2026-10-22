namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class Layover
    {
        public string arrivalFlightNum { get; set; }
        public ArrivalMarketingCarrier arrivalMarketingCarrier { get; set; }
        public ArrivalOperatingCarrier arrivalOperatingCarrier { get; set; }
        public bool changeOfAirport { get; set; }
        public string departureFlightNum { get; set; }
        public DepartureMarketingCarrier departureMarketingCarrier { get; set; }
        public DepartureOperatingCarrier departureOperatingCarrier { get; set; }
        public string destinationAirportCode { get; set; }
        public bool equipmentChange { get; set; }
        public string layoverAirportCode { get; set; }
        public LayoverDuration layoverDuration { get; set; }
        public string originAirportCode { get; set; }
        public string scheduledArrivalLocalTs { get; set; }
        public string scheduledDepartureLocalTs { get; set; }
    }
}
