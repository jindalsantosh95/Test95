using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class TaxDetail
    {
        [JsonProperty("TFCC")] public string TFCC { get; set; }
        [JsonProperty("TFCA", DefaultValueHandling = DefaultValueHandling.Include)] public int TFCA { get; set; }
        [JsonProperty("TFCS")] public string TFCS { get; set; }
    }
}
