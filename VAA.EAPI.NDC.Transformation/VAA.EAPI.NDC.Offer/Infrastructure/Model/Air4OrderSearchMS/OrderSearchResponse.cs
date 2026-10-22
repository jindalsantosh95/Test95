using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS
{
    public class OrderSearchResponse
    {
        public IEnumerable<OriginDestinationAssociations> OriginDestinationAssociations { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<OrderInstructions> OrderInstructions { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
        public IEnumerable<ProductComponent> ProductComponents { get; set; }
        public PointOfSale PointOfSale { get; set; }
        public UtcAndLocalDateTime CreationDateTime { get; set; }
        public OrderNumber OrderNumber { get; set; }
        public BookingReference BookingReferences { get; set; }
        public IEnumerable<TimeLimit> TimeLimits { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public CurrencyAmount TotalPrice { get; set; }
        public string OrderStatus { get; set; }
        public bool IsCabinUpgraded { get; set; }
        public string UpgradedCabinDesc { get; set; }
    }
}
