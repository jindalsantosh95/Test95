using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability
{
    public class SearchCriteria
    {
        public Parties Parties { get; set; }
        public Pos Pos { get; set; }
        public Context Context { get; set; }
        public IEnumerable<Segment> FlightSegments { get; set; }
        public string OfferId { get; set; }
        public string OrderId { get; set; }
        public string ChannelId { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
    }
}
