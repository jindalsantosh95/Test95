namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.Air4Payment
{
    public class PaymentMethod
    {
        public BillingAddress BillingAddress { get; set; }
        public PaymentCard PaymentCard { get; set; }
        public PaymentPurchaseAdvisoryMessage PaymentPurchaseAdvisoryMessage { get; set; }
        public Authorization Authorization { get; set; }
        public string PaymentMethodTypeName { get; set; }


    }

}