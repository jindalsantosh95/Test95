using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OfferDataList
    {
        public IEnumerable<Passenger> Customers { get; set; }
        public IEnumerable<OriginDestinationTrip> OriginDestinationTrips { get; set; }
        public IEnumerable<Trip> Trips { get; set; }
        public IEnumerable<FlightSegment> FlightSegments { get; set; }
        public IEnumerable<Trip> OriginalTrips { get; set; }
        public IEnumerable<FlightSegment> OriginalFlightSegments { get; set; }
        public IEnumerable<RetailItemDefinition> RetailItemDefinitionList { get; set; }
        public OfferPreferences OfferPreferences { get; set; }
        public IEnumerable<PricingOption> PricingOptions { get; set; }
        public IEnumerable<Penalty> Penalties { get; set; }
        public IEnumerable<BaggageAllowanceWithIdentifier> BaggageAllowances { get; set; }
        public ResponseProperties ResponseProperties { get; set; }
        public IEnumerable<AccountableDocument> AccountableDocuments { get; set; }
        public OfferDataListEcreditEvaluationResponse EcreditEvaluationResponse { get; set; }
        public OfferDataListRetainSeatsResponse RetainSeatsResponse { get; set; }
        public AccountableDocumentSummary OriginalAccountableDocumentInNewCurrency { get; set; }
        public IEnumerable<string> FlexDestinationAirportCodes { get; set; }
        public IEnumerable<string> FlexReturnDates { get; set; }
    }
}
