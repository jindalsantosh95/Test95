using System.Collections;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class SegmentDetails
    {
        public string AirCraft { get; set; }
        public string MarketingCarrier { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OperatingCarrier { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string OriginDestinationRefId { get; set; }
        public string OriginAirportCode { get; set; }
        public string OriginAirportName { get; set; }
        public string OriginCityCode { get; set; }
        public string OriginCountryCode { get; set; }
        public string OriginCountryName { get; set; }
        public string DestinationAirportCode { get; set; }
        public string DestinationAirportName { get; set; }
        public string DestinationCityCode { get; set; }
        public string DestinationCountryCode { get; set; }
        public string DestinationCountryName { get; set; }
        public string ClassOfService { get; set; }
        public ScheduledArrivalTime ScheduledArrivalTime { get; set; }
        public ScheduledDepartureTime ScheduledDepartureTime { get; set; }
        public IEnumerable<Legs> Legs { get; set; }
        public string SegmentNumber { get; set; }
        public string SegmentId { get; set; }
        public string NumberInParty { get; set; }
        public string FlightDuration { get; set; }
        public FlightDistance FlightDistance { get; set; }
        public Action Status { get; set; }
        public CabinType CabinType { get; set; }
        public string CassEquipment { get; set; }
    }
}
