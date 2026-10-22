using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.RefundTicket
{
    public class VoidTransactionDetails
    {
        public string Pnr { get; set; }
        public string IssueDate { get; set; }
        public List<TicketDetails> Tickets { get; set; }
        public PnrSearch OrderData { get; set; }
    }
}
