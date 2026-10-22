using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Pricing
    {
        public string PricingOptionId { get; set; }
        public Price TotalAmt { get; set; }
        public Price BaseAmt { get; set; }
        public TaxSummary TaxSummary { get; set; }
        public IEnumerable<Discount> DiscountsApplied { get; set; }
        public IEnumerable<PromotionalPrice> PromotionalPrices { get; set; }
        public Price OriginalTotalAmt { get; set; }
        public PricingRepriceQuoteAmt RepriceQuoteAmt { get; set; }
        public IEnumerable<LoyaltyBalance> MilesEarned { get; set; }
    }
}
