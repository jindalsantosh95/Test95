using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.SCVResults;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders
{
    public class OrderAssociationRequestData
    {
        public string CustomerId { get; set; }

        public List<OrderCollectionResult> Orders { get; set; }
    }
}
