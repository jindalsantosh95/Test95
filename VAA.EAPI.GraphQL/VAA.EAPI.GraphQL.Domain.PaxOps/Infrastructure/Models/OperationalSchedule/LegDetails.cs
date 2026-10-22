using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.OperationalSchedule
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

        [JsonProperty("Actual_Departure_Time")]
        public OperationScheduleDateTime ActualDepartureTime { get; set; }

        [JsonProperty("Actual_Arrival_Time")]
        public OperationScheduleDateTime ActualArrivalTime { get; set; }

        [JsonProperty("Estimated_Departure_Time")]
        public OperationScheduleDateTime EstimatedDepartureTime { get; set; }

        [JsonProperty("Estimated_Arrival_Time")]
        public OperationScheduleDateTime EstimatedArrivalTime { get; set; }

        [JsonProperty("Wheels_Up")]
        public OperationScheduleDateTime WheelsUp { get; set; }

        [JsonProperty("Wheels_Down")]
        public OperationScheduleDateTime WheelsDown { get; set; }

        [JsonProperty("Departure_Status")]
        public string DepartureStatus { get; set; }

        [JsonProperty("Arrival_Status")]
        public string ArrivalStatus { get; set; }

        [JsonProperty("Flight_Cancelled")]
        public string FlightCancelled { get; set; }

        public Compartment[] Compartments { get; set; }
        public object[] DelayCodes { get; set; }

        [JsonProperty("Wheels_Up_Status")]
        public string WheelsUpStatus { get; set; }

        [JsonProperty("Wheels_Down_Status")]
        public string WheelsDownStatus { get; set; }
    }



}
