using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel
{
    public class DeleteOrder
    {
        public string OrderId { get; set; }
        public BookingReference BookingReference { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }
}
