using System.Collections.Generic;
using VAA.EAPI.Web.Search.Models.OfferMS;

namespace VAA.EAPI.Web.Search.Models.GraphQl
{
    public class FlightOfferRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public FlightSearchRequest FlightSearchRequest { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public string BasketId { get; set; }
        public string SortRequest { get; set; }
        public FilterRequest Filters { get; set; }
    }
}
