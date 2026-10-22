namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment
{
    public class OrderPaymentResponse
    {
        public string CorrelationId { get; set; }
        public string Status { get; set; }
        public string Data { get; set; }
    }
}
