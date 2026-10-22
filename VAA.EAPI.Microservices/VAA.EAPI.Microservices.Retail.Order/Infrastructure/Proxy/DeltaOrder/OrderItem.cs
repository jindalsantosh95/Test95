using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class OrderItem
    {
        public string OrderItemNum { get; set; }
        public string ProductCategoryId { get; set; }
        public OrderItemOwner OrderItemOwner { get; set; }
        public DateTime OrderItemCreationUtcTs { get; set; }
        public string OrderItemStatusCode { get; set; }
        public OrderItemPricing OrderItemPricing { get; set; }
        public List<RetailItem> RetailItems { get; set; }
        public IEnumerable<Info> Errors { get; set; }
    }
}