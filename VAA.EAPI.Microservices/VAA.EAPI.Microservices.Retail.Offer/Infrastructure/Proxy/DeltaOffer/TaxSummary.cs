using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class TaxSummary
    {
        public Price TotalTaxAmt { get; set; }
        public IEnumerable<TaxType> Taxes { get; set; }
    }
}
