namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class OriginDestinationTrip
    {
        public string originDestinationId { get; set; }
        public string originAirportCode { get; set; }
        public string destinationAirportCode { get; set; }
        public List<string> tripIds { get; set; }
    }
}
