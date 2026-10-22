using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.OperationalSchedule
{
    public class OperationScheduleDateTime
    {
        [JsonProperty("Local_Date_Time")]
        public DateTime LocalDateTime { get; set; }
        [JsonProperty("UTC_Date_Time")]
        public DateTime UTCDateTime { get; set; }
    }
}
