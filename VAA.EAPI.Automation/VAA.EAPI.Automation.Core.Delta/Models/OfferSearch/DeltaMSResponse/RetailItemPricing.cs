namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class RetailItemPricing
    {
        public string pricingOptionId { get; set; }
        public TotalAmt totalAmt { get; set; }
        public BaseAmt baseAmt { get; set; }
        public TaxSummary taxSummary { get; set; }
        public List<object> promotionalPrices { get; set; }
    }
}
