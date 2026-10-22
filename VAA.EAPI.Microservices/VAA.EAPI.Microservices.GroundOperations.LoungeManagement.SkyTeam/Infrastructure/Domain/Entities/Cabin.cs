using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities
{
    public class Cabin
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("fareCode")]
        public string FareCode { get; set; }
    }
}
