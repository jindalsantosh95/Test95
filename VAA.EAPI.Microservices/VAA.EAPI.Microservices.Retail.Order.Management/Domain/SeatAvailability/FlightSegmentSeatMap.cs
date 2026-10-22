using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Products.Seat;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.SeatAvailability
{
    public class FlightSegmentSeatMap
    {
        public Segment FlightSegment { get; set; }
        public IEnumerable<FlightSegmentSeatMapCatalogue> Catalogue { get; set; }
        public IEnumerable<SeatMap> Seatmap { get; set; }
    }
}
