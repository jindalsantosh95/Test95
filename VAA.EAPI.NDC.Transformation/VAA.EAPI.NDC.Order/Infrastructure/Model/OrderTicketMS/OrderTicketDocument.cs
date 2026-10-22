using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Common;
using ErrorDetails = VAA.EAPI.Domain.Entities.Common.ErrorDetails;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS
{
    public class OrderTicketDocument
    {
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
        public Pos PointOfSale { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRef { get; set; }
        public IEnumerable<TicketSegment> TicketSegmentDetails { get; set; }
        public string TransactionId {  get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public IEnumerable<OrderInstructions> OrderInstructions { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }

    }
}
