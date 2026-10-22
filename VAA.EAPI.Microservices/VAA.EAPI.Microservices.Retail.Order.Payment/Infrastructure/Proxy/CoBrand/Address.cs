using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand
{
    public class Address
    {
        [JsonProperty("cipher.zipCode")]
        public string CipherzipCode { get; set; }

        [JsonProperty("cipher.addressLine1")]
        public string CipheraddressLine1 { get; set; }

        [JsonProperty("cipher.addressLine2")]
        public string CipheraddressLine2 { get; set; }
        
        [JsonProperty("cipher.city")]
        public string Ciphercity { get; set; }

        [JsonProperty("cipher.state")]
        public string Cipherstate { get; set; }
       
    }
}
