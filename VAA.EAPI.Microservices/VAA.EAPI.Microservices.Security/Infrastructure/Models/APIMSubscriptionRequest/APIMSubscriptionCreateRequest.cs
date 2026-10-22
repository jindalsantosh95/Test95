using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMSubscriptionRequest
{
    public class APIMSubscriptionCreateRequest
    {
        [JsonProperty(PropertyName = "properties")]
        public SubscriptionCreateProperties Properties { get; set; }

    }
}
