using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class Record4
    {
        [JsonProperty("PRFR")] public string PRFR { get; set; }
        [JsonProperty("TODC")] public string TODC { get; set; }
        [JsonProperty("OITD")] public string OITD { get; set; }
        [JsonProperty("OILC")] public string OILC { get; set; }
        [JsonProperty("OIDT")] public string OIDT { get; set; }
        [JsonProperty("OIAN")] public string OIAN { get; set; }
        [JsonProperty("TCDE")] public string TCDE { get; set; }
        [JsonProperty("BFAM")] public string BFAM { get; set; }
        [JsonProperty("TAXA")] public string TAXA { get; set; }
        [JsonProperty("TAXA2")] public string TAXA2 { get; set; }
        [JsonProperty("TAXA3")] public string TAXA3 { get; set; }
        [JsonProperty("TTDS")] public string TTDS { get; set; }
        [JsonProperty("SASP")] public string SASP { get; set; }
        [JsonProperty("BAID")] public string BAID { get; set; }
        [JsonProperty("ANCD")] public string ANCD { get; set; }
        [JsonProperty("BFAM_CURRENCY")] public string BFAM_CURRENCY { get; set; }
    }

}
