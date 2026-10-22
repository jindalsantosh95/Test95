using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Customer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Order;
using VAA.EAPI.GraphQL.Domain.Book.Models.Ticket;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight
{
    public class BookingDetails
    {
        public IEnumerable<Passenger> Passengers { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
        public Pos PointOfSale { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRef { get; set; }
        public IEnumerable<TicketSegment> TicketSegments { get; set; }

    }
}
