using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities
{
    public class ConfirmationDetails
    {
        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("entityAirlineCode")]
        public string EntityAirlineCode { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }
}
