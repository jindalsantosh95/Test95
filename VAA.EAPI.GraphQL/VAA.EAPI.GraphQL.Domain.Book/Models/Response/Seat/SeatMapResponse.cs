using VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Seat
{
    public class SeatMapResponse
    {
        public Segment FlightSegment { get; set; }
        public IEnumerable<SeatDetails> FlightPassengerSeatMap { get; set; }
        public string CabinClass { get; set; }
        public SeatMap SeatMap { get; set; }
        public string FareFamily { get; set; }
        public FareProduct FareProduct { get; set; }
    }
}