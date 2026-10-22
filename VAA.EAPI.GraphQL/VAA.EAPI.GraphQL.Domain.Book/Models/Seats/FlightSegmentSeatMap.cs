using VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Seats
{
    public class FlightSegmentSeatMap
    {
        public Segment FlightSegment { get; set; }
        public IEnumerable<FlightSegmentSeatMapCatalogue> Catalogue { get; set; }
        public IEnumerable<SeatMap> Seatmap { get; set; }
    }
}
