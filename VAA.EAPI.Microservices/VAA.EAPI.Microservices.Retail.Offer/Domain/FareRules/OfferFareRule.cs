using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.FareRules
{
    public class OfferFareRule
    {
        public IEnumerable<OfferFareRuleCategory> OfferdFareRuleCategoryList { get; set; }
        public string FareBasisCode { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }


        public static explicit operator OfferFareRule(OfferRule source)
        {
            if (source == null)
                return null;

            return new()
            {
                DestinationAirportCode = source.DestinationAirportCode,
                OfferdFareRuleCategoryList = source.OfferRuleCategoryList.Select(rc => (OfferFareRuleCategory)rc),
                FareBasisCode = source.FareBasisCode,
                OriginAirportCode = source.OriginAirportCode
            };
        }
    }
}
