using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders
{
    public class FlightOrders
    {
        public OrderNumber OrderId { get; set; }
        public LocalAndUTCDateTime CreationDateTime { get; set; }
        public List<BookingReference> BookingReferences { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }
}
