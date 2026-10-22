namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class PaymentMethod
    {
        public string PaymentMethodTypeName { get; set; }
        public PaymentCard PaymentCard { get; set; }
        public BillingAdr BillingAddress { get; set; }
        public FinancialInstitution FinancialInstitution { get; set; }
        public Miles Miles { get; set; }
    }
}
