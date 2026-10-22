using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class PassengerItinerarySegment
    {
        public string CompartmentCode { get; set; }
        public string CompartmentName { get; set; }
        public DestinationAirport DestinationAirport { get; set; }
        public string DestinationAirportCode { get; set; }
        public List<FlightLeg> FlightLeg { get; set; }
        public string FlightSegmentNum { get; set; }
        public MarketingCarrier MarketingCarrier { get; set; }
        public string MarketingFlightNum { get; set; }
        public OperatingCarrier OperatingCarrier { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string OperatingFlightNum { get; set; }
        public OriginAirport OriginAirport { get; set; }
        public string OriginAirportCode { get; set; }
        public DateTime ScheduledArrivalLocalTs { get; set; }
        public DateTime ScheduledDepartureLocalTs { get; set; }
        public TotalAirTime TotalAirTime { get; set; }
        public string FlightSegmentId { get; set; }
        public string MarketingClassOfServiceCode { get; set; }
        public bool Arnk { get; set; }
    }
}
