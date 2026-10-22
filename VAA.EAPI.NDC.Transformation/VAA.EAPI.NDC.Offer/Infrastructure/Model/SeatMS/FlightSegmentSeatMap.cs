using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Seat;


namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS
{
    public class FlightSegmentSeatMap
    {
        public Segment FlightSegment { get; set; }
        public IEnumerable<FlightSegmentSeatMapCatalogue> Catalogue { get; set; }
        public IEnumerable<Domain.Model.VAA.SeatMap> Seatmap { get; set; }
    }
}
