namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference
{
    public class MilesEquivalentPrice
    {
        public bool CashPlusMiles { get; set; }
        public int DiscountMileCount { get; set; }
        public string MileCount { get; set; }
    }
}