using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch
{
    public class OriginDestination
    {
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
        public string DepartureDate { get; set; }
    }
}
