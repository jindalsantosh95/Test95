using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsFlightSegmentSeatMap")]
    public class FlightSegmentSeatMap
    {
        public Segment FlightSegment { get; set; }
        public IEnumerable<FlightSegmentSeatMapCatalogue> Catalogue { get; set; }
        public IEnumerable<SeatMap> Seatmap { get; set; }
    }
}