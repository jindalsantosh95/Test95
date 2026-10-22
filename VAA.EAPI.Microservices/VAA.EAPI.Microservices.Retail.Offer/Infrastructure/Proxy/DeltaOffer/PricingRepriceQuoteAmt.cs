using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class PricingRepriceQuoteAmt
    {
        public Price AdditionalCollectionAmt { get; set; }
        public CountryOfCommencementFare CountryOfCommencementFare { get; set; }
        public Price CancelPenaltyAmt { get; set; }
        public Price ChangePenaltyAmt { get; set; }
        public Price ExternalReissueChargePenaltyAmt { get; set; }
        public Price FareDifferenceAmt { get; set; }
        public Price RefundAmt { get; set; }
        public Price TotalPenaltyAmt { get; set; }
        public Price TotalTaxDifferenceAmt { get; set; }
        public IEnumerable<TravelRelatedServiceTax> ValueAddTaxAmt { get; set; }
    }
}
