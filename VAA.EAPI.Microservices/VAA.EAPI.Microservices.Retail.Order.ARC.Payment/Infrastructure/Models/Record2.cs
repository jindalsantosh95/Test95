using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class Record2
    {
        [JsonProperty("ANCD")] public string ANCD { get; set; }
        [JsonProperty("CUIN")] public string CUIN { get; set; }
        [JsonProperty("TDOI")] public string TDOI { get; set; }
        [JsonProperty("SCDE")] public string SCDE { get; set; }
        [JsonProperty("TDNM")] public string TDNM { get; set; }
        [JsonProperty("DTYP_TCDF")] public string DTYP_TCDF { get; set; }
        [JsonProperty("SASP")] public string SASP { get; set; }
        [JsonProperty("PNME")] public string PNME { get; set; }
        [JsonProperty("TDTM")] public string TDTM { get; set; }
        [JsonProperty("SACD")] public string SACD { get; set; }
        [JsonProperty("TICC")] public string TICC { get; set; }
    }

}
