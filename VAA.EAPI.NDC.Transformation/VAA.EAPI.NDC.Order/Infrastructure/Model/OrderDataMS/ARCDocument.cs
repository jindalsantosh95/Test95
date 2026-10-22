using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS
{
    public class ARCDocument
    {
        public BookingReference BookingReference { get; set; }
        public IEnumerable<ARCTransactionHistory> ARCTransactionHistory { get; set; }
    }
}
