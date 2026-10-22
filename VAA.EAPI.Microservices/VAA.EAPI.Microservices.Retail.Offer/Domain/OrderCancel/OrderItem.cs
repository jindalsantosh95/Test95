using System.Collections.Generic;
namespace VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel
{
    public class OrderItem
    {
        public string OrderItemId { get; set; }
        public List<string> ProductRetention { get; set; }
    }
}
