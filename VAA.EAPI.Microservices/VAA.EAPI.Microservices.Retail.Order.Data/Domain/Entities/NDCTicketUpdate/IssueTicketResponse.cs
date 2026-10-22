using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCTicketUpdate
{
    public class IssueTicketResponse
    {
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
        public Pos PointOfSale { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRef { get; set; }
        public IEnumerable<TicketSegment> TicketSegmentDetails { get; set; }
        public string TransactionId { set; get; }
        public IEnumerable<PriceClass> PriceClass { get; set; }
        public ChangeTypeEnum? ChangeType { get; set; }
        public string RequestType { set; get; }
    }
}
