using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity
{
    public class FarePrice
    {
        public List<object> ApprovalCode { get; set; }
        public List<FarePriceDetail> FarePriceDetails { get; set; }
        public List<DtiSegmentInfo> DtiSegmentInfo { get; set; }
    }
}
