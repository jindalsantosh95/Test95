using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS
{
    public class ARCTransactionHistory
    {
       public IEnumerable<PassengerTicketDetails> PassengerTicketDetails { get; set; }
        public string CustomerId { get; set; }
    }
}
