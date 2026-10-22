using Newtonsoft.Json;

namespace VAA.Facade.AzureB2C.Domain.Model
{
    public class Identity
    {
        [JsonProperty(PropertyName = "signInType")]
        public string SignInType { get; set; }

        [JsonProperty(PropertyName = "issuer")]
        public string Issuer { get; set; }

        [JsonProperty(PropertyName = "issuerAssignedId")]
        public string IssuerAssignedId { get; set; }
    }
}
