using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.Common
{
    public class JweRsaKey
    {
        [JsonProperty("kty")]
        public string Kty { get; set; }

        [JsonProperty("n")]
        public string N { get; set; }

        [JsonProperty("e")]
        public string E { get; set; }

        [JsonProperty("kid")]
        public string Kid { get; set; }

        [JsonProperty("key_ops")]
        public string[] KeyOps { get; set; }
    }
}
