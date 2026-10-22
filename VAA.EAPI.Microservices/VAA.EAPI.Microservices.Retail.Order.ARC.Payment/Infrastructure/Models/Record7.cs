using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class Record7
    {
        [JsonProperty("FCAR")] public string FCAR { get; set; }
        [JsonProperty("FCSN")] public int FCSN { get; set; }
        [JsonProperty("FCAR2")] public string FCAR2 { get; set; }
        [JsonProperty("FCSN2")] public int FCSN2 { get; set; }
        [JsonProperty("FCPI")] public string FCPI { get; set; }
        [JsonProperty("FCMI")] public string FCMI { get; set; }
    }
}
