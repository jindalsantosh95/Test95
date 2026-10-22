namespace VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.PaymentStatus
{
    public class Request
    {
        public string basketId { get; set; }
        public string paymentReferenceId { get; set; }
        public string hppPaymentStatus { get; set; }
    }
}
