namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class OfferDataList
    {
        public List<Customer> customers { get; set; }
        public List<OriginDestinationTrip> originDestinationTrips { get; set; }
        public List<Trip> trips { get; set; }
        public List<FlightSegment> flightSegments { get; set; }
        public List<RetailItemDefinitionList> retailItemDefinitionList { get; set; }
        public OfferPreferences offerPreferences { get; set; }
        public List<PricingOption> pricingOptions { get; set; }
        public List<Penalty> penalties { get; set; }
        public List<object> baggageAllowances { get; set; }
        public ResponseProperties responseProperties { get; set; }
    }
}
