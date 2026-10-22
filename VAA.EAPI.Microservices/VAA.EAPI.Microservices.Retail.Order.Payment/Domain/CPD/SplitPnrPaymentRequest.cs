using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD
{
    public class SplitPnrPaymentRequest
    {
        public string OrderId { get; set; }
        public BookingReference ParentBookingReference { get; set; }
        public BookingReference ChildBookingReference { get; set; }
        public IEnumerable<string> SplitPaxIds { get; set; }
    }
}
