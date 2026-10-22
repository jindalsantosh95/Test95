using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class Record9
    {
        [JsonProperty("ERST")] public string ERST { get; set; }
        [JsonProperty("FOPI")] public string FOPI { get; set; }
    }
}
