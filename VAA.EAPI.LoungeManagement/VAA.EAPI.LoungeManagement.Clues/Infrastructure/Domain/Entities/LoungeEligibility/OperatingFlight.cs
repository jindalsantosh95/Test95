using Newtonsoft.Json;
using System;

namespace VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility
{
    public class OperatingFlight
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("departureTime")]
        public string DepartureTime { get; set; }

        [JsonProperty("arrivalTime")]
        public string ArrivalTime { get; set; }

        [JsonProperty("cabin")]
        public Cabin Cabin { get; set; }

        [JsonProperty("carrier")]
        public Carrier Carrier { get; set; }
    }
}
