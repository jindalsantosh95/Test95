using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Products.Seat;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.SeatAvailability
{
    public class FlightSegmentSeatMap
    {
        public Segment FlightSegment { get; set; }
        public List<FlightSegmentSeatMapCatalogue> Catalogue { get; set; }
        public List<SeatMap> Seatmap { get; set; }
    }
}
