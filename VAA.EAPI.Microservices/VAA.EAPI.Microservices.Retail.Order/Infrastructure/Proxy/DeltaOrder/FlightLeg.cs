using System;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class FlightLeg
    {
        public Aircraft Aircraft { get; set; }
        public string CarrierCode { get; set; }
        public bool CodeShareOnly { get; set; }
        public bool ConnectionCarrierOnly { get; set; }
        public DestinationAirport DestinationAirport { get; set; }
        public string DestinationAirportCode { get; set; }
        public Distance Distance { get; set; }
        public string LegId { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string OperatingCarrierCode { get; set; }
        public OriginAirport OriginAirport { get; set; }
        public string OriginAirportCode { get; set; }
        public DateTime ScheduledArrivalLocalTs { get; set; }
        public DateTime ScheduledDepartureLocalTs { get; set; }
        public string MarketingClassOfService { get; set; }
    }
}
