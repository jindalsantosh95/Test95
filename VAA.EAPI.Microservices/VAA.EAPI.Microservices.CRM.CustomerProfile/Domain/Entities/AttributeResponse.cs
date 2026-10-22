using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities
{
    public class AttributeResponse
    {
        [JsonProperty("amperity_id")]
        public string AmperityId { get; set; }

        [JsonProperty("loyalty_id")]
        public string LoyaltyId { get; set; }
    }
}
