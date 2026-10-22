using Newtonsoft.Json;

namespace VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility
{
    public class Membership
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("loyaltyProgram")]
        public LoyaltyProgram LoyaltyProgram { get; set; }
    }
}
