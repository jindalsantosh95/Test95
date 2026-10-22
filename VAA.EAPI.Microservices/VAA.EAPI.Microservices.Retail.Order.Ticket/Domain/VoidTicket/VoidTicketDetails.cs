using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.VoidTicket
{
    public class VoidTicketDetails
    {
        public string PNR { get; set; }
        public string IssueDate { get; set; }

        public List<CouponDetail> Tickets { get; set; }


    }
}
