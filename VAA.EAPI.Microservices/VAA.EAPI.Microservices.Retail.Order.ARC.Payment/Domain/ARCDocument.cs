using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain
{
    public class ARCDocument
    {
        public BookingReference BookingReference { get; set; }
        public IEnumerable<ARCTransactionHistory> ARCTransactionHistory { get; set; }
    }
}
