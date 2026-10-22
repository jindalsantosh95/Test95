using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class DlOrderResponse
    {
        public List<Order> Orders { get; set; }
    }
}