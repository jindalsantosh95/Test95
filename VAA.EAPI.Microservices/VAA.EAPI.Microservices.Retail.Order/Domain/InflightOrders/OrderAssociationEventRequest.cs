using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders
{
    public class OrderAssociationEventRequest : CloudEvent
    {
        public new OrderAssociationRequestData Data { get; set; }
    }
}
