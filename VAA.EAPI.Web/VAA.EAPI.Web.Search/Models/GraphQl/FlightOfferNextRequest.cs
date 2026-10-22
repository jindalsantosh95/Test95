using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.OfferMS
{
    public class FlightOfferNextRequest
    {
        public string? BasketId { get; set; }
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public FlightSearchRequest FlightSearchRequest { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public string SortRequest { get; set; }
        public FilterRequest Filters { get; set; }
    }
}
