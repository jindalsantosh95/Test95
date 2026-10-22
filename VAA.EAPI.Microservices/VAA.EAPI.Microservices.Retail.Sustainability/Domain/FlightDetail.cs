using System;
namespace VAA.EAPI.Microservices.Retail.Sustainability.Domain
{
    public sealed class FlightDetail
    {
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string FlightNumber { get; set; }
        public CabinType? Cabin { get; set; }
        public DateTime FlightDate { get; set; }
        public string RequestId { get; set; }
    }
}
