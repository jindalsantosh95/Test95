using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Customer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Seats
{
    public class SeatMSRequest
    {
        public Parties Parties { get; set; }
        public Pos Pos { get; set; }
        public Context Context { get; set; }
        public IEnumerable<Segment> FlightSegments { get; set; }
        public string OfferId { get; set; }
        public string OrderId { get; set; }
        public string ChannelId { get; set; }
        public IEnumerable<CustomerInfo> CustomerDetails { get; set; }
    }
}