namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.PaymentRequest
{
    public class OrderPaymentRequest
    {
        public string BasketId { get; set; }
        public decimal AmountToPay { get; set; }
        public string IpAddress { get; set; }
        public string DeviceInfo { get; set; }
        public string BrowserInfo { get; set; }
        public string PaymentSuccessLink { get; set; }
        public string PaymentFailureLink { get; set; }
        public string PayWithPointsValue { get; set; }
    }
}
