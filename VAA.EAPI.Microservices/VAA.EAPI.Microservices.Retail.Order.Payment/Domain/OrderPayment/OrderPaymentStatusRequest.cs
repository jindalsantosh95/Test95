namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment
{
    public class OrderPaymentStatusRequest
    {
        public string OrderReference { get; set; }
        public string Status { get; set; }
        public string PayLoad { get; set; }


    }
}
