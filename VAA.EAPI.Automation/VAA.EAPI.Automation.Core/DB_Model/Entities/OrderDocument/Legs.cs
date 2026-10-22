using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class Legs
    {
        [JsonProperty("ScheduledDepartureTime")]
        public ScheduledDepartureTime ScheduledDepartureTime { get; set; }

        [JsonProperty("LegSequenceNumber")]
        public string LegSequenceNumber { get; set; }

        [JsonProperty("OriginAirportCode")]
        public string OriginAirportCode { get; set; }

        [JsonProperty("OperatingCarrier")]
        public string OperatingCarrier { get; set; }

        [JsonProperty("ScheduledArrivalTime")]
        public ScheduledArrivalTime ScheduledArrivalTime { get; set; }

        [JsonProperty("DestinationAirportCode")]
        public string DestinationAirportCode { get; set; }
    }
}