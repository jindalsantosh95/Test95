using Newtonsoft.Json.Linq;
using VAA.EAPI.Microservices.FlightOperations.Flights.Extensions;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusPerformance
    {
        public int? MilesFlown { get; set; }
        public bool? OnTime { get; set; }
        public bool? ArrivalDelay30 { get; set; }
        public bool? ArrivalDelay60 { get; set; }
        public bool? Cancellation { get; set; }

        public static explicit operator FlightStatusPerformance(JObject statsSource) => new()
        {
            MilesFlown = statsSource.ToInt("milesFlown", "miles"),
            OnTime = statsSource.ToBool("onTime", "isOnTime"),
            ArrivalDelay30 = statsSource.ToBool("arrivalDelay30", "delayed30"),
            ArrivalDelay60 = statsSource.ToBool("arrivalDelay60", "delayed60"),
            Cancellation = statsSource.ToBool("cancellation", "cancelled", "isCancelled")
        };
    }
}
