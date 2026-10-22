using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OfferRule
    {
        public IEnumerable<OfferRuleCategory> OfferRuleCategoryList { get; set; }
        public string FareBasisCode { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
    }
}
