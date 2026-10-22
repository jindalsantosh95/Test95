namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch
{
    public class OffersCriteria
    {
        public Preferences preferences { get; set; }
        public PricingCriteria pricingCriteria { get; set; }
        public int resultsPageNum { get; set; }
        public int resultsPerRequestNum { get; set; }
        public FlightRequestCriteria flightRequestCriteria { get; set; }
    }
}