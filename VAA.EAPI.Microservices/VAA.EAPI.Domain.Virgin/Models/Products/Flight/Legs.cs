using Newtonsoft.Json.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Flight
{
    public class Legs
    {
        public UtcAndLocalDateTime ScheduledDepartureTime { get; set; }
        public string LegSequenceNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string OperatingCarrier { get; set; }
        public string MarketingCarrier { get; set; }
        public UtcAndLocalDateTime ScheduledArrivalTime { get; set; }
        public string DestinationAirportCode { get; set; }
        public string LegDuration { get; set; }
        public string StopOverDuration { get; set; }
        public bool? IsDominantLeg { get; set; }
        public string? LegId { get; set; }
        public string OriginTerminal { get; set; }
        public string DestinationTerminal { get; set; }
        public string CarrierAircraftTypeCode { get; set; }
        public string CarrierAircraftTypeName { get; set; }
        public FlightDistance Distance { get; set; }

        public static explicit operator Legs(JObject source)
        {
            return new Legs()
            {
                ScheduledDepartureTime = source["ScheduledDepartureTime"] != null ? (UtcAndLocalDateTime)source["ScheduledDepartureTime"] : null,
                LegSequenceNumber = source["LegSequenceNumber"]?.ToString(),
                OriginAirportCode = source["OriginAirportCode"]?.ToString(),
                OperatingCarrier = source["OperatingCarrier"]?.ToString(),
                MarketingCarrier = source["MarketingCarrier"]?.ToString(),
                ScheduledArrivalTime = source["ScheduledArrivalTime"] != null ? (UtcAndLocalDateTime)source["ScheduledArrivalTime"] : null,
                DestinationAirportCode = source["DestinationAirportCode"]?.ToString(),
                LegDuration = source["LegDuration"]?.ToString(),
                StopOverDuration = source["StopOverDuration"]?.ToString(),
                IsDominantLeg = source["IsDominantLeg"] != null ? (bool)source["IsDominantLeg"] : null,
                LegId = source["LegId"]?.ToString(),
                OriginTerminal = source["OriginTerminal"]?.ToString(),
                DestinationTerminal = source["DestinationTerminal"]?.ToString(),
                CarrierAircraftTypeCode = source["CarrierAircraftTypeCode"]?.ToString(),
                CarrierAircraftTypeName = source["CarrierAircraftTypeName"]?.ToString(),
                Distance = source["Distance"] != null ? (FlightDistance)source["Distance"] : null,
            };
        }
    }
}
