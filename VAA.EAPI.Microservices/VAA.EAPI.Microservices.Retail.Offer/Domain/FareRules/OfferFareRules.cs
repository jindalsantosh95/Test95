using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.FareRules
{
    public class OfferFareRules
    {
        public IEnumerable<OfferFareRule> FareRules { get; set; }


        public static explicit operator OfferFareRules(OfferRuleList source)
        {
            if (source == null)
                return null;

            return new()
            {
                FareRules = source.OfferRules.Select(rule => (OfferFareRule)rule)
            };
        }
    }

}
