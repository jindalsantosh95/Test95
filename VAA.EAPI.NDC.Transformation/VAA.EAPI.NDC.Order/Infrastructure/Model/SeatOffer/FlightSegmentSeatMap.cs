using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Seat;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatOffer
{
    public class FlightSegmentSeatMap
    {
        public FlightSegment FlightSegment { get; set; }
        public IEnumerable<Catalogue> Catalogue { get; set; }
        public IEnumerable<SeatMap> Seatmap { get; set; }
    }
}
