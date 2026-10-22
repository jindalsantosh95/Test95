using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class Payer
    {
        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("LoyaltyMemberTravelling")]
        public bool LoyaltyMemberTravelling { get; set; }

        [JsonProperty("LoyaltyMemberDetails")]
        public LoyaltyMemberDetails LoyaltyMemberDetails { get; set; }

        [JsonProperty("PayerType")]
        public string PayerType { get; set; }
    }
}