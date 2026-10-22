namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class OfferPreferences
    {
        public List<int> stopCnt { get; set; }
        public List<object> destinationAirportCode { get; set; }
        public ConnectionTimeRange connectionTimeRange { get; set; }
        public List<object> originAirportCode { get; set; }
        public FlightDurationRange flightDurationRange { get; set; }
        public List<string> layoverAirportCode { get; set; }
        public TotalMilesRange totalMilesRange { get; set; }
        public TotalPriceRange totalPriceRange { get; set; }
    }
}
