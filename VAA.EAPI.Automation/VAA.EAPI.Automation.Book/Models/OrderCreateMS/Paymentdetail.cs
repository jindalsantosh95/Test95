namespace VAA.EAPI.Automation.Book.Models.OrderCreateMS
{
    public class Paymentdetail
    {
        public string formOfPaymentId { get; set; }
        public Amount Amount { get; set; }
        public CardMethod CardMethod { get; set; }
    }
}
