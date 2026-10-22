using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class PricingPreference
    {
        public IEnumerable<string> PriceableIn { get; set; }
    }
}
