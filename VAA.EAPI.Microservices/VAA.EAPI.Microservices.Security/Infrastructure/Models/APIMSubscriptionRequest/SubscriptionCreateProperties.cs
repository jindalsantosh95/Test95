using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMSubscriptionRequest
{
    public class SubscriptionCreateProperties
    {
        public string OwnerId { get; set; }
        public string Scope { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }
        public string State { get; set; }

    }

}
