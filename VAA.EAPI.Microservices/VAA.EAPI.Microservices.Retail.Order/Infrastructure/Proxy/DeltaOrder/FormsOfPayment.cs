namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class FormsOfPayment
    {
        public string FormOfPaymentId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public Price Price { get; set; }
        public string PaymentReferenceId { get; set; }
        public AuthenticationData AuthenticationData { get; set; }
    }
}