namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class CurrencyEquivalentPrice
    {
        public int decimalPrecisionCnt { get; set; }
        public double currencyAmt { get; set; }
        public string currencyCode { get; set; }
        public double roundedCurrencyAmt { get; set; }
        public string formattedCurrencyAmt { get; set; }
        public string formattedRoundedCurrencyAmt { get; set; }
    }
}
