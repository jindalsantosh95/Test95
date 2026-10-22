using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class PricingCriteria
    {
        public IEnumerable<string> PriceableIn { get; set; }
        public string CurrencyCode { get; set; }
        public string DisplayCurrencyCode { get; set; }
        public bool WaiveChangeFee { get; set; }
        public Price OfferAmount { get; set; }
    }
}
