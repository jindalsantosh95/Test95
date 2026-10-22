namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class OfferPricing
    {
        public TotalAmt totalAmt { get; set; }
        public BaseAmt baseAmt { get; set; }
        public TaxSummary taxSummary { get; set; }
        public List<object> promotionalPrices { get; set; }
        public OriginalTotalAmt originalTotalAmt { get; set; }
    }
}
