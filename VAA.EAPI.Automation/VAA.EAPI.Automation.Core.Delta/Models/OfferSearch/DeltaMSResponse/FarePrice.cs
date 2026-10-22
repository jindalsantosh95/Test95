namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class FarePrice
    {
        public BaseFare baseFare { get; set; }
        public SalesPrice salesPrice { get; set; }
        public List<TotalFarePassengerPrice> totalFarePassengerPrice { get; set; }
        public TotalFarePrice totalFarePrice { get; set; }
        public OriginalTotalPriceByPTC originalTotalPriceByPTC { get; set; }
        public List<OriginalTotalPaxPrice> originalTotalPaxPrice { get; set; }
        public TotalFarePricePerPtc totalFarePricePerPtc { get; set; }
        public TotalTaxAndFees totalTaxAndFees { get; set; }
        public List<object> promotionalTotalPrice { get; set; }
        public List<object> promotionalTotalPriceByPTC { get; set; }
        public List<object> promotionalTotalPaxPrice { get; set; }
        public List<object> promotionalPrices { get; set; }
        public List<object> promotionalPricesByPTC { get; set; }
        public List<object> promotionalPaxPrices { get; set; }
    }
}
