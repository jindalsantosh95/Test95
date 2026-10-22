namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.MPI
{

    public class Currency
    {
        public string CountryCode { get; set; }

        public string CurrencyAmt { get; set; }

        public string CurrencyCode { get; set; }

        public int DecimalPrecisionCnt { get; set; }

        public int IataDecimalPrecisionCnt { get; set; }

    }
}
