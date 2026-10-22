using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand
{

    public class Contactinfo
    {
        [JsonProperty("cipher.contactType")]
        public string CiphercontactType { get; set; }
        [JsonProperty("cipher.contactNumber")]
        public string CiphercontactNumber { get; set; }
    }
}
