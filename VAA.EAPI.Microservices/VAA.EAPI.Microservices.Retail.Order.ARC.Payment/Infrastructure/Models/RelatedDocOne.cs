using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class RelatedDocOne
    {
        [JsonProperty("CUIN")] public string CUIN { get; set; }
        [JsonProperty("VCRR")] public string VCRR { get; set; }
        [JsonProperty("TDNM")] public string TDNM { get; set; }
    }
}
