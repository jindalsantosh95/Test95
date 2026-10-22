namespace VAA.EAPI.Automation.Core.Delta.Models.OrderCreate
{
    public class FormsOfPayment
    {
        public string formOfPaymentId { get; set; }
        public PaymentMethod paymentMethod { get; set; }
        public Price price { get; set; }
    }
}