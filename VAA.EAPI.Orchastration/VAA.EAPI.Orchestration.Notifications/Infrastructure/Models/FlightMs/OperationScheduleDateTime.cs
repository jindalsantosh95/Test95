using Newtonsoft.Json;
using System;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs
{
    public class OperationScheduleDateTime
    {
        [JsonProperty("Local_Date_Time")]
        public DateTime LocalDateTime { get; set; }
        [JsonProperty("UTC_Date_Time")]
        public DateTime UTCDateTime { get; set; }
    }
}
