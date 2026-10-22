using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities
{
    public class LoyaltyStatus
    {
        [JsonProperty("tierLevel")]
        public string TierLevel { get; set; }

        [JsonProperty("allianceTierLevel")]
        public string AllianceTierLevel { get; set; }

        [JsonProperty("ffpContractStatus")]
        public string FfpContractStatus { get; set; }
    }
}
