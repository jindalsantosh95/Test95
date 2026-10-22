using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain
{
    public class ARCTransactionHistory
    {
        public IEnumerable<PassengerTicketDetails> PassengerTicketDetails { get; set; }
        public string CustomerId { get; set; }
    }
}
