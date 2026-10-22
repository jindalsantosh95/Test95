using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMSubscriptionRequest
{
    public class APIMSubscriptionUpdateRequest
    {
        [JsonProperty(PropertyName = "properties")]
        public SubscriptionUpdateProperties Properties { get; set; }

    }
}
