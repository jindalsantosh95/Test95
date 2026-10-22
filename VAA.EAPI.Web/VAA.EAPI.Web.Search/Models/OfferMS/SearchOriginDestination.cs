using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.OfferMS
{
    public class SearchOriginDestination
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string DepartureDate { get; set; }
        public DepartureTimeRange DepartureTimeRange { get; set; }
        public IEnumerable<string> ConnectionAirports { get; set; }
    }
}
