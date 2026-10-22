using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class OrderSearchResponse
    {
        public IEnumerable<OriginDestinationAssociations> OriginDestinationAssociations { get; set; }
        public IEnumerable<CustomerDetails> CustomerDetails { get; set; }
        public IEnumerable<OrderInstructions> OrderInstructions { get; set; }
        public IEnumerable<SegmentDetails> SegmentDetails { get; set; }
        public IEnumerable<ProductComponents> ProductComponents { get; set; }
        public PointOfSale PointOfSale { get; set; }
        public CreationDateTime CreationDateTime { get; set; }
        public OrderNumber OrderNumber { get; set; }
        public BookingReferences BookingReferences { get; set; }
        public IEnumerable<TimeLimit> TimeLimits { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public CurrencyAmount TotalPrice { get; set; }
        public string OrderStatus { get; set; }
        public bool IsCabinUpgraded { get; set; }
        public string UpgradedCabinDesc { get; set; }
    }
}
