namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference
{
    public class Amount
    {
        public string MonetaryAmount { get; set; }
        public int DecimalPrecisionCount { get; set; }
        public string Currency { get; set; }
    }
}