using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class RecordA
    {
        [JsonProperty("ICWD")] public string ICWD { get; set; }
        [JsonProperty("RFIC")] public string RFIC { get; set; }
    }
}
