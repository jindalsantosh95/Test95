namespace VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Entities
{
    public class OrderNotificationResponse
    {
        public string RequestId { get; set; }
        public OrderDetails OrderDetails { get; set; }
    }
}
