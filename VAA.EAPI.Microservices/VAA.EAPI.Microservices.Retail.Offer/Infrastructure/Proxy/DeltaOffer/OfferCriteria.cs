using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OfferCriteria
    {
        public string SelectedOfferId { get; set; }
        public PricingCriteria PricingCriteria { get; set; }
        public Qualifiers Qualifiers { get; set; }
        public int ResultsPageNum { get; set; }
        public int ResultsPerRequestNum { get; set; }
        public Dictionary<string, string> AdditionalCriteriaMap { get; set; }
    }
}
