namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.Air4Payment
{
    public class CurrencyEquivalentPrice
    {
        public decimal CurrencyAmt { get; set; }
        public string CurrencyCode { get; set; }
        public int DecimalPrecisionCnt { get; set; }
        public int IataDecimalPrecisionCnt { get; set; }
    }

}