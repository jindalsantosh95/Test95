using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class LoyaltyMemberDetails
    {
        [JsonProperty("MemberType")]
        public string MemberType { get; set; }

        [JsonProperty("MemberNumber")]
        public string MemberNumber { get; set; }

        [JsonProperty("PayWithPoints")]
        public bool PayWithPoints { get; set; }
    }
}