using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using DomainCustomer = VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Domain.Virgin.Models.Order
{
    public class OrderView
    {
        public List<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public List<DomainCustomer.Customer> Customer { get; set; }
        public List<OrderInstructions> OrderInstructions { get; set; }
        public List<Segment> SegmentDetails { get; set; }
        public Pos PointOfSale { get; set; }
        public OrderNumber OrderNumber { get; set; }
        public BookingReference BookingReferences { get; set; }
        public List<TimeLimit> TimeLimits { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public List<OrderItems> OrderItems { get; set; }
        public string OrderStatus { get; set; }
        public bool IsCabinUpgraded { get; set; }
        public string UpgradedCabinDesc { get; set; }
        public List<Warning> Warnings { get; set; }

    }
}
