using System.Collections.Generic;
using System.Security.Policy;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Domain
{
    public class IssueTicketResponse
    {
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
        public Pos PointOfSale { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRef { get; set; }
        public IEnumerable<TicketSegment> TicketSegmentDetails { get; set; }
        public string TransactionId { get; set; }
        public IEnumerable<PriceClass> PriceClass { get; set; }
        public ChangeTypeEnum ChangeType { get;  set; }
        public string RequestType { get; set; }
        public IEnumerable<OrderInstructions> OrderInstructions { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
    }
}
