using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity
{
    public class OrderDoc
    {
        public string Id { get; set; }
        public string OrderID { get; set; }
        public string OrderHexAddress { get; set; }
        public string OrderVersion { get; set; }
        public string DocType { get; set; }
        public string BookingCancelledIndicator { get; set; }
        public OrderDetails OrderDetails { get; set; }
        public OriginalPOS OriginalPOS { get; set; }
        public List<CustomerDetail> CustomerDetails { get; set; }
        public List<SegmentDetail> SegmentDetails { get; set; }
        public List<SpecialServiceRequest> SpecialServiceRequest { get; set; }
        public List<AncillaryOrder> AncillaryOrders { get; set; }
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
