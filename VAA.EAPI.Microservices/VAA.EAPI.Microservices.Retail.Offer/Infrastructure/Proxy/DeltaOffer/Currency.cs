namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Currency
    {
        public int DecimalPrecisionCnt { get; set; }
        public string CountryCode { get; set; }
        public decimal CurrencyAmt { get; set; }
        public string CurrencyCode { get; set; }
        public decimal RoundedCurrencyAmt { get; set; }
        public string FormattedCurrencyAmt { get; set; }
        public string FormattedRoundedCurrencyAmt { get; set; }
    }
}
