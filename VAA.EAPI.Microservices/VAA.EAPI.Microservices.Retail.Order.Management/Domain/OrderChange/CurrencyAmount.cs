namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange
{
    public class CurrencyAmount
    {
        public string MonetaryAmount { get; set; }
        public string BaseCurrency { get; set; }
        public int DecimalPrecisionCount { get; set; }
    }
}
