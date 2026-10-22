using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class FormOfPayment
    {
        [JsonProperty("FOPA")] public string FOPA { get; set; }
        [JsonProperty("FOPB", DefaultValueHandling = DefaultValueHandling.Include) ] public int FOPB { get; set; }
        [JsonProperty("ACDE")] public string ACDE { get; set; }
        [JsonProperty("FOPT")] public string FOPT { get; set; }
        [JsonProperty("EDCC")] public string EDCC { get; set; }
        [JsonProperty("SOAD")] public string SOAD { get; set; }
        [JsonProperty("CCTI")] public string CCTI { get; set; }
        [JsonProperty("AAMN")] public int? AAMN { get; set; }
    }
}
