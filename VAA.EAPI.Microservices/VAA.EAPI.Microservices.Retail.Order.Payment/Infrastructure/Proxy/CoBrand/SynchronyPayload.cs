using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand
{
    public class SynchronyPayload
    {
        [JsonProperty("cipher.payload")]
        public string Cipherpayload { get; set; }

        public static explicit operator SynchronyPayload(string sourcePayload)
        {
            return new SynchronyPayload()
            {
                Cipherpayload = sourcePayload
            };
        }
    }
}
