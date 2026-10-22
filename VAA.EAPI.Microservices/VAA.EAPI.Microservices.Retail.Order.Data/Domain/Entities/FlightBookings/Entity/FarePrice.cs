using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class FarePrice
    {
        public IEnumerable<object> ApprovalCode { get; set; }
        public IEnumerable<FarePriceDetail> FarePriceDetails { get; set; }
        public IEnumerable<DtiSegmentInfo> DtiSegmentInfo { get; set; }
    }
}
