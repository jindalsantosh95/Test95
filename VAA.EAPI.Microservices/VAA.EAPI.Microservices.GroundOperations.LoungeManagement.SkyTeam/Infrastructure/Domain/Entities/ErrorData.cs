using Newtonsoft.Json;
namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities
{
    public class ErrorData
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
