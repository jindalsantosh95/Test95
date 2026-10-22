using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities
{
    public class Membership
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("loyaltyProgram")]
        public LoyaltyProgram LoyaltyProgram { get; set; }
    }
}
