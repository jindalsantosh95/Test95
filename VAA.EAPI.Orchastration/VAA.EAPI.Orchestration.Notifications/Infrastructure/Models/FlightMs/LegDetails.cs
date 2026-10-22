using Newtonsoft.Json;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs
{
    public class LegDetails
    {
        [JsonProperty("Leg_Id")]
        public string LegId { get; set; }
        [JsonProperty("Origin_Airport_Code")]
        public string OriginAirportCode { get; set; }
        [JsonProperty("Destination_Airport_Code")]
        public string DestinationAirportCode { get; set; }

        [JsonProperty("Scheduled_Departure_Time")]
        public OperationScheduleDateTime ScheduledDepartureTime { get; set; }
        [JsonProperty("Scheduled_Arrival_Time")]
        public OperationScheduleDateTime ScheduledArrivalTime { get; set; }

    }
}