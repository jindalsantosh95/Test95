namespace VAA.EAPI.Web.Book.Models.Book.Payment
{
    public class PaymentSession
    {
        public decimal AmountToPay { get; set; }
        public string Status { get; set; }
        public PaymentAmount PaymentAmount { get; set; }
        public string PaymentLink { get; set; }
        public string RemoveLink { get; set; }
        public string ParentPaymentReferenceId { get; set; }
        public string PaymentReferenceId { get; set; }
        public string HppPaymentStatus { get; set; }
    }
}