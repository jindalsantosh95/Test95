namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.Air4Payment
{

    public class FormsOfPayment
    {
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentReferenceId { get; set; }
        public string ParentPaymentReferenceId { get; set; }
        public Price Price { get; set; }
        public Authorization Authorization { get; set; }
        public string VendorId { get; set; }
        public PurchaseInfo PurchaseInfo { get; set; }
    }
}