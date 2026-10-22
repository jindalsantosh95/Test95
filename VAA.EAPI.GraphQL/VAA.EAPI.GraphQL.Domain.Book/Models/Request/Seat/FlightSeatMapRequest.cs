
namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Seat
{
    public class FlightSeatMapRequest
    {
        public string Pos { get; set; }
        public string CurrencyCode { get; set; }
        public FlightSegment FlightSegment { get; set; }
        public string OfferId { get; set; }
        public string OrderId { get; set; }
        public string ChannelId { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }

    }
}