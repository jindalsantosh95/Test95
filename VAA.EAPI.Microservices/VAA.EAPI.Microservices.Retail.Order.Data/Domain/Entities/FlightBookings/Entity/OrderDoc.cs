
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class OrderDoc
    {
        public string Id { get; set; }
        public string OrderID { get; set; }
        public string OrderHexAddress { get; set; }
        public string OrderVersion { get; set; }
        public string DocType { get; set; }
        public string BookingCancelledIndicator { get; set; }
        public OrderDetail OrderDetails { get; set; }
        public OriginalPOS OriginalPOS { get; set; }
        public IEnumerable<CustomerDetail> CustomerDetails { get; set; }
        public IEnumerable<SegmentDetail> SegmentDetails { get; set; }
        public IEnumerable<SpecialServiceRequest> SpecialServiceRequest { get; set; }
        public IEnumerable<AncillaryOrder> AncillaryOrders { get; set; }
        public Fulfillment Fulfillment { get; set; }
        public OrderHistorydetails OrderHistorydetails { get; set; }
        public string SourceSystem { get; set; }
        public string Rid { get; set; }
        public string Self { get; set; }
        public string Etag { get; set; }
        public string Attachments { get; set; }
        public int Ts { get; set; }
    }
}
