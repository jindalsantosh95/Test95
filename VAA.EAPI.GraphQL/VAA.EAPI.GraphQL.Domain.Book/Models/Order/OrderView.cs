//using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight;
using DomainCustomer = VAA.EAPI.GraphQL.Domain.Book.Models.Customer;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Order
{
    public class OrderView
    {
        public IEnumerable<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public IEnumerable<DomainCustomer.CustomerInfo> Customers { get; set; }
        public IEnumerable<OrderInstructions> OrderInstructions { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
        public Pos PointOfSale { get; set; }
        public OrderNumber OrderNumber { get; set; }
        public BookingReference BookingReferences { get; set; }
        public IEnumerable<TimeLimit> TimeLimits { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public string OrderStatus { get; set; }
        public bool IsCabinUpgraded { get; set; }
        public string UpgradedCabinDesc { get; set; }
        //public IEnumerable<Warnings> Warnings { get; set; }

    }
}
