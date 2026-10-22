using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.OperationalSchedule
{
    public class Flight
    {
        public Aircraft Aircraft { get; set; }

        [JsonProperty("Marketing_Flight_Number")]
        public MarketingFlightNumber MarketingFlightNumber { get; set; }

        [JsonProperty("Operating_Flight_Number")]
        public OperatingFlightNumber OperatingFlightNumber { get; set; }
        public IEnumerable<LegDetails> Legs { get; set; }

        [JsonProperty("Turn_Around")]
        public string TurnAround { get; set; }
    }
}
