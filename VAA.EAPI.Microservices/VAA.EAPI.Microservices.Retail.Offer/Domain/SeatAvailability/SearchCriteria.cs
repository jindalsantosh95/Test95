using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.SeatAvailability
{
    public class SearchCriteria
    {
        public Context Context { get; set; }
        public List<Segment> FlightSegments { get; set; }
        public string OfferId { get; set; }
        public string OrderId { get; set; }
    }
}
