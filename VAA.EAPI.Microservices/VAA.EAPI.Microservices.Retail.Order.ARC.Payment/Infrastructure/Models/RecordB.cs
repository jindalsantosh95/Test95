using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class RecordB
    {
        [JsonProperty("CIDO")] public string CIDO { get; set; }
    }
}
