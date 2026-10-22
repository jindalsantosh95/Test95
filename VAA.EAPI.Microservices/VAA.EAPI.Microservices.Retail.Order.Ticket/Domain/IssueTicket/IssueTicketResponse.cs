using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.IssueTicket
{
    public class IssueTicketResponse
    {
        public List<Customer> CustomerDetails { get; set; }
        public List<Segment> SegmentDetails { get; set; }
        public Pos PointOfSale { get; set; }
        public BookingReference BookingReference { get; set; }
        public List<AccountableDocumentRef> AccountableDocumentRef { get; set; }
        public List<TicketSegment> TicketSegmentDetails { get; set; }
    }
}
