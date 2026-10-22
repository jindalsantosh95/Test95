using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand
{

    public class Membershipinfo
    {
        [JsonProperty("cipher.rewardsNumber")]
        public string CipherrewardsNumber { get; set; }
        public string MemberSinceDate { get; set; }
        [JsonProperty("cipher.memberNumber")]
        public string CiphermemberNumber { get; set; }
        public string ReissueDate { get; set; }
        [JsonProperty("cipher.auMembershipNumber")]
        public string CipherauMembershipNumber { get; set; }
        [JsonProperty("cipher.alternateCustomerId")]
        public string CipheralternateCustomerId { get; set; }
    }
}
