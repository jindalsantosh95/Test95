using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OffersCriteria
    {
        public OfferToComplement OfferToComplement { get; set; }
        public string RecordLocatorId { get; set; }
        public IEnumerable<PassengerAccountableDocumentAssociation> PassengerAccountableDocumentAssociations { get; set; }
        public OrderToComplement OrderToComplement { get; set; }
        public OfferPreferences Preferences { get; set; }
        public Qualifiers Qualifiers { get; set; }
        public PricingCriteria PricingCriteria { get; set; }
        public bool AlternateOffer { get; set; }
        public bool SearchWithContext { get; set; }
        public int ResultsPageNum { get; set; }
        public int ResultsPerRequestNum { get; set; }
        public FlightOfferCriteria FlightRequestCriteria { get; set; }
        public IEnumerable<SeatOfferCriteria> SeatCriteria { get; set; }
        public IEnumerable<BagOfferCriteria> BagOfferCriteria { get; set; }
        public Dictionary<string, string> AdditionalCriteriaMap { get; set; }
    }
}
