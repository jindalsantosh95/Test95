using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange
{
    public class OrderChangeEventRequest : CloudEvent
    {
        public new OrderChangeRequestData Data { get; set; }
    }
}
