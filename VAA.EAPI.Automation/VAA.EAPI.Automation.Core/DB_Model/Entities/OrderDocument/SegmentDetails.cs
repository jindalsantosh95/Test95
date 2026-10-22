using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class SegmentDetails
    {
        [JsonProperty("OdRefId")]
        public string OdRefId { get; set; }

        [JsonProperty("SegmentId")]
        public string SegmentId { get; set; }

        [JsonProperty("MarketingCarrier")]
        public string MarketingCarrier { get; set; }

        [JsonProperty("MarketingFlightNumber")]
        public string MarketingFlightNumber { get; set; }

        [JsonProperty("OperatingCarrier")]
        public string OperatingCarrier { get; set; }

        [JsonProperty("OperatingFlightNumber")]
        public string OperatingFlightNumber { get; set; }

        [JsonProperty("OriginAirportCode")]
        public string OriginAirportCode { get; set; }

        [JsonProperty("OriginCityCode")]
        public string OriginCityCode { get; set; }

        [JsonProperty("ClassOfService")]
        public string ClassOfService { get; set; }

        [JsonProperty("ScheduledArrivalTime")]
        public ScheduledArrivalTime ScheduledArrivalTime { get; set; }

        [JsonProperty("ScheduledDepartureTime")]
        public ScheduledDepartureTime ScheduledDepartureTime { get; set; }

        [JsonProperty("SegmentNumber")]
        public string SegmentNumber { get; set; }

        [JsonProperty("Legs")]
        public Legs[] Legs { get; set; }
    }
}