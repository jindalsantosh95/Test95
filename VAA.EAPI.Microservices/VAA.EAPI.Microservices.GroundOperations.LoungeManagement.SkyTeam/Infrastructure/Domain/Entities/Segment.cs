using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities
{
    public class Segment
    {
        [JsonProperty("origin")]
        public string Origin { get; set; }

        [JsonProperty("destination")]
        public string Destination { get; set; }

        [JsonProperty("operatingFlight")]
        public OperatingFlight OperatingFlight { get; set; }

        [JsonProperty("isTravellingFlight")]
        public bool IsTravellingFlight { get; set; }
    }
}
