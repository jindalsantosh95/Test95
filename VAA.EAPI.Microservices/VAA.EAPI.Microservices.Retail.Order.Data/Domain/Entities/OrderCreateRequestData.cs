using MediatR;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities
{
    public class OrderCreateRequestData : IRequest<Unit>
    {
        public string Channel { get; set; }
        public OrderNotificationRequest NotificationRequest { get; set; }
    }
}
