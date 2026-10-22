using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities
{
    public class Carrier
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
