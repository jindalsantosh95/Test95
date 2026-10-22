using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class Trip
    {
        public string DestinationAirportCode { get; set; }
        public Distance Distance { get; set; }
        public string OriginAirportCode { get; set; }
        public TotalTripTime TotalTripTime { get; set; }
        public string TripBrandId { get; set; }
        public string TripId { get; set; }
        public List<string> FlightSegmentIdList { get; set; }
    }
}
