using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMUserRequest
{
    public class UserCreationRequest
    {
        [JsonProperty(PropertyName = "properties")]
        public UserProperties Properties { get; set; }
    }
}
