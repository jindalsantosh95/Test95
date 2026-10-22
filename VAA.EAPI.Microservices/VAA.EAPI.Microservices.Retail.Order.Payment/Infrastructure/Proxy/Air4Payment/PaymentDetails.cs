namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.Air4Payment
{
    public class Air4PaymentDetails
    {
        public string PaymentReferenceId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public Price Price { get; set; }
        public string parentPaymentReferenceId { get; set; }

    }

}