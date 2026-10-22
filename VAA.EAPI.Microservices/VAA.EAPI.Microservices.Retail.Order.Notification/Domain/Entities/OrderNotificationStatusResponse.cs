namespace VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Entities
{
    public class OrderNotificationStatusResponse
    {
        public string CorrelationId { get; set; }
        public string Status { get; set; }
        public string Payload { get; set; }
    }
}
