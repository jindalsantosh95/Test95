using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class SearchOriginDestination
    {
        public string ArrivalLocalTs { get; set; }
        public string ArrivalUtcTs { get; set; }
        public IEnumerable<string> ConnectionAirportCode { get; set; }
        public string DepartureLocalTs { get; set; }
        public string DepartureUtcTs { get; set; }
        public IEnumerable<Airport> Destinations { get; set; }
        public IEnumerable<string> FlightNum { get; set; }
        public string TripId { get; set; }
        public IEnumerable<Airport> Origins { get; set; }
        public string TransactionLocalTs { get; set; }
        public string TransactionUtcTs { get; set; }
        public CalendarDateCriteria CalenderDateRequest { get; set; }
    }
}
