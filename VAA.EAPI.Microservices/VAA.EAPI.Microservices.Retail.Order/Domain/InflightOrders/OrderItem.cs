using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders
{
    public class OrderItem
    {
        public string OrderItemId { get; set; }
        public ExternalOrderReferences ExternalReferences { get; set; }
        public List<FlightProduct> Services { get; set; }
    }
}
