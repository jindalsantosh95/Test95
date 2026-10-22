using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.SCVResults
{
    public class OrderCollectionResult
    {
        public string OrderID { get; set; }
        public string OwnerAirline { get; set; }
        public string DocType { get; set; }
        public CreationDateTime CreationDateTime { get; set; }
        public List<BookingReference> BookingReferences { get; set; }
        public List<SegmentDetail> SegmentDetails { get; set; }
        public List<CustomerDetail> CustomerDetails { get; set; }
    }
}
