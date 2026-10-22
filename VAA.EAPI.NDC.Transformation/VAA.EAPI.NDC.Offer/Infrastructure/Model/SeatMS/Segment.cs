using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Offer.Mapper.SeatAvailability;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS
{
    public class Segment
    {
        public string OdRefId { get; set; }
        public string SegmentId { get; set; }
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
        public UtcAndLocalDateTime ScheduledArrivalTime { get; set; }
        public UtcAndLocalDateTime ScheduledDepartureTime { get; set; }
        public string SegmentNumber { get; set; }
        public string NumberInParty { get; set; }
        public string FlightDuration { get; set; }
        public IEnumerable<Legs> Legs { get; set; }
        public CabinType CabinType { get; set; }
        public IEnumerable<CompartmentAvailability> Compartment { get; set; }
        public IEnumerable<ClassAvailability> Class { get; set; }
        public string FareBasis { get; set; }
        public string FareFamily { get; set; }
        public FlightDistance FlightDistance { get; set; }
        public string ConnectionOrStopOver { get; set; }
    }
}
