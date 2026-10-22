using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Flight
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
        public List<CompartmentAvailability> Compartment { get; set; }
        public List<ClassAvailability> Class { get; set; }
        public string FareBasis { get; set; }
        public string FareFamily { get; set; }
        public FlightDistance FlightDistance { get; set; }
        public string ConnectionOrStopOver { get; set; }
        public string ConnectionDuration { get; set; }
        public string EquipmentCode { get; set; }
        public int SegmentSequence { get; set; }
        public string ServiceDefinitionsRefID { get; set; }
        public bool? PendingGovtApproval { get; set; }
        public Status Status { get; set; }
        public string TicketNotValidAfter { get; set; }
        public string TicketNotValidBefore { get; set; }
        public bool ScheduleChangeIndicator {  get; set; }

        public static explicit operator Segment(JObject source)
        {
            return new Segment()
            {
                OdRefId = source["OdRefId"]?.ToString(),
                SegmentId = source["SegmentId"]?.ToString(),
                AirCraft = source["AirCraft"]?.ToString(),
                MarketingCarrier = source["MarketingCarrier"]?.ToString(),
                MarketingFlightNumber = source["MarketingFlightNumber"]?.ToString(),
                OperatingCarrier = source["OperatingCarrier"]?.ToString(),
                OperatingFlightNumber = source["OperatingFlightNumber"]?.ToString(),
                OriginDestinationRefId = source["OriginDestinationRefId"]?.ToString(),
                OriginAirportCode = source["OriginAirportCode"]?.ToString(),
                OriginAirportName = source["OriginAirportName"]?.ToString(),
                OriginCityCode = source["OriginCityCode"]?.ToString(),
                OriginCountryCode = source["OriginCountryCode"]?.ToString(),
                OriginCountryName = source["OriginCountryName"]?.ToString(),
                DestinationAirportCode = source["DestinationAirportCode"]?.ToString(),
                DestinationAirportName = source["DestinationAirportName"]?.ToString(),
                DestinationCityCode = source["DestinationCityCode"]?.ToString(),
                DestinationCountryCode = source["DestinationCountryCode"]?.ToString(),
                DestinationCountryName = source["DestinationCountryName"]?.ToString(),
                ClassOfService = source["ClassOfService"]?.ToString(),
                ScheduledArrivalTime = source["ScheduledArrivalTime"] != null ? (UtcAndLocalDateTime)source["ScheduledArrivalTime"] : null,
                ScheduledDepartureTime = source["ScheduledDepartureTime"] != null ? (UtcAndLocalDateTime)source["ScheduledDepartureTime"] : null,
                SegmentNumber = source["SegmentNumber"]?.ToString(),
                NumberInParty = source["NumberInParty"]?.ToString(),
                FlightDuration = source["FlightDuration"]?.ToString(),
                Legs = source["Legs"]?.Select(a => (Legs)a),
                CabinType = source["CabinType"] != null ? (CabinType)source["CabinType"] : null,
                Compartment = source["Compartment"]?.Select(a => (CompartmentAvailability)a).ToList(),
                Class = source["Class"]?.Select(a => (ClassAvailability)a).ToList(),
                FareBasis = source["FareBasis"]?.ToString(),
                FareFamily = source["FareFamily"]?.ToString(),
                FlightDistance = source["FlightDistance"] != null ? (FlightDistance)source["FlightDistance"] : null,
                ConnectionOrStopOver = source["ConnectionOrStopOver"]?.ToString(),
                ConnectionDuration = source["ConnectionDuration"]?.ToString(),
                EquipmentCode = source["EquipmentCode"]?.ToString(),
                SegmentSequence = source["SegmentSequence"] != null ? (int)source["SegmentSequence"] : 0,
                ServiceDefinitionsRefID = source["ServiceDefinitionsRefID"]?.ToString(),
                Status = source["Status"] != null ? (Status)source["Status"] : null,
                TicketNotValidAfter = source["TicketNotValidAfter"]?.ToString(),
                TicketNotValidBefore = source["TicketNotValidBefore"]?.ToString()
            };
        }
    }
}
