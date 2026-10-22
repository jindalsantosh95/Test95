using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Microservices.Retail.Order.Create.Domain
{
    public class OrderResponse
    {
        public List<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public List<Customer> CustomerDetails { get; set; }
        public List<OrderInstructions> OrderInstructions { get; set; }
        public List<Segment> SegmentDetails { get; set; }
        public Pos PointOfSale { get; set; }
        public UtcAndLocalDateTime CreationDateTime { get; set; }
        public OrderNumber OrderNumber { get; set; }
        public BookingReference BookingReference { get; set; }
        public List<TimeLimit> TimeLimits { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public List<OrderItems> OrderItems { get; set; }
        public string OrderStatus { get; set; }
    }
}
