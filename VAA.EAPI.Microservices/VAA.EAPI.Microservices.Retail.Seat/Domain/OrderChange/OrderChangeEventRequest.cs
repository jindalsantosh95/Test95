using VAA.EAPI.Domain.Events;
namespace VAA.EAPI.Microservices.Retail.Seat.Domain.OrderChange
{
    public class OrderChangeEventRequest : CloudEvent
    {
        public new OrderChangeRequestData Data { get; set; }
    }
}
