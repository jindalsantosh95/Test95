using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Order;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Order
{
    public class OrderResponseMS
    {
        public IEnumerable<OrderInstructions> OrderInstructions { get; set; }
        public Pos PointOfSale { get; set; }
        public UtcAndLocalDateTime CreationDateTime { get; set; }
        public OrderNumber OrderNumber { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<TimeLimit> TimeLimits { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public string OrderStatus { get; set; }
        public DataList DataList { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
        public string BookingType { get; set; }
        public string OrderType { get; set; }
    }
}