using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCArcDocumentUpdate
{
    public class ARCTransactionHistory
    {
        public IEnumerable<PassengerTicketDetails> PassengerTicketDetails { get; set; }
        public string CustomerId { get; set; }

    }
}
