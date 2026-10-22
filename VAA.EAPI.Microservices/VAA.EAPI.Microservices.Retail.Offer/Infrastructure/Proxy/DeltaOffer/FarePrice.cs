using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class FarePrice
    {
        public string PricingOptionId { get; set; }
        public Price BaseFare { get; set; }
        public Price BaseFareOtherCurrency { get; set; }
        public Price MilesEquivalentPrice { get; set; }
        public Price SalesPrice { get; set; }
        public IEnumerable<Price> TotalFarePassengerPrice { get; set; }
        public Price TotalFarePrice { get; set; }
        public Price TotalFarePriceInOtherCurrency { get; set; }
        public IEnumerable<Discount> DiscountsApplied { get; set; }
        public Price OriginalTotalPrice { get; set; }
        public Price OriginalTotalPriceByPTC { get; set; }
        public IEnumerable<Price> OriginalTotalPaxPrice { get; set; }
        public Price TotalPriceInOtherCurrency { get; set; }
        public Price OriginalTotalPriceInOtherCurrency { get; set; }
        public Price TotalFarePricePerPtc { get; set; }
        public Price TotalFarePublishedPrice { get; set; }
        public Price TotalTaxAndFees { get; set; }
        public Price TotalTaxDifference { get; set; }
        public IEnumerable<PromotionalPrice> PromotionalTotalPrice { get; set; }
        public IEnumerable<PromotionalPrice> PromotionalTotalPriceByPTC { get; set; }
        public IEnumerable<PromotionalPrice> PromotionalTotalPaxPrice { get; set; }
        public IEnumerable<PromotionalPrice> PromotionalPrices { get; set; }
        public IEnumerable<PromotionalPrice> PromotionalPricesByPTC { get; set; }
        public IEnumerable<PromotionalPrice> PromotionalPaxPrices { get; set; }
    }
}
