using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public partial class Legs
    {
        [JsonProperty("LegSequenceNumber")]
        public string? LegSequenceNumber { get; set; }

        [JsonProperty("OriginAirportCode")]
        public string? OriginAirportCode { get; set; }

        [JsonProperty("Destination_Airport_Code")]
        public string? Destination_Airport_Code { get; set; }

        [JsonProperty("ScheduledDepartureTime")]
        public ScheduledDepartureTime? ScheduledDepartureTime { get; set; }

        [JsonProperty("ScheduledArrivalTime")]
        public ScheduledArrivalTime? ScheduledArrivalTime { get; set; }

        [JsonProperty("EquipmentType")]
        public string? EquipmentType { get; set; }

        [JsonProperty("LegDistance")]
        public LegDistance? LegDistance { get; set; }

        [JsonProperty("LegDuration")]
        public string? LegDuration { get; set; }

        [JsonProperty("DelegatedCarrier")]
        public string? DelegatedCarrier { get; set; }

        [JsonProperty("OperatingCarrier")]
        public string? OperatingCarrier { get; set; }
    }
}
