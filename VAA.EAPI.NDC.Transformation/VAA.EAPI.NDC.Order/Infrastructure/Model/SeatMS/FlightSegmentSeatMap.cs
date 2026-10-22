using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Seat;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS
{
    public class FlightSegmentSeatMap
    {
        public Segment FlightSegment { get; set; }
        public IEnumerable<FlightSegmentSeatMapCatalogue> Catalogue { get; set; }
        public IEnumerable<SeatMap> Seatmap { get; set; }
    }
}
