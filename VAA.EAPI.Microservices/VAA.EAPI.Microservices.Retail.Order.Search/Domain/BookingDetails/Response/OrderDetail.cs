using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Response
{
    public class OrderDetail
    {
        public string OrderID { get; set; }

        public SegmentDetail SegmentDetails { get; set; }

        public List<CustomerDetail> CustomerDetails { get; set; }

        public OrderDetails Order { get; set; }

        public OriginalPOS OriginalPos { get; set; }
    }
}
