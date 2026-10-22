using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order
{
    public class OrderResponse
    {
        public DataList DataList { get; set; }
        public Pos PointOfSale { get; set; }
        public UtcAndLocalDateTime CreationDateTime { get; set; }
        public OrderNumber OrderNumber { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<TimeLimit> TimeLimits { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public string OrderStatus { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
        public string BookingType { get; set; }
        public string OrderType { get; set; }
    }
}
