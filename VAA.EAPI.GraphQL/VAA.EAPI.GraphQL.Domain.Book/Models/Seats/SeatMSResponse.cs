using VAA.EAPI.GraphQL.Domain.Book.Models.Common;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Seats
{
    public class SeatMSResponse
    {
        public Parties Parties { get; set; }
        public Pos Pos { get; set; }
        public string SeatOfferId { get; set; }
        public string OfferExpirationTimeLimit { get; set; }
        public IEnumerable<FlightSegmentSeatMap> FlightSegmentSeatMap { get; set; }
        public string OrderId { get; set; }
    }
}