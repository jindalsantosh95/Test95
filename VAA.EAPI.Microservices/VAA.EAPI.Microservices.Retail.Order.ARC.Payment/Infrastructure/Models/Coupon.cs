using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class Coupon
    {
        [JsonProperty("SOAC")] public string SOAC { get; set; }
        [JsonProperty("SDAC")] public string SDAC { get; set; }
        [JsonProperty("SADS")] public string SADS { get; set; }
        [JsonProperty("SRBD")] public string SRBD { get; set; }
        [JsonProperty("SFDD_DOSR")] public string SFDD_DOSR { get; set; }
        [JsonProperty("NVBD")] public string NVBD { get; set; }
        [JsonProperty("NVAD")] public string NVAD { get; set; }
        [JsonProperty("FBSS")] public string FBSS { get; set; }
        [JsonProperty("SFNM")] public string SFNM { get; set; }
        [JsonProperty("SFDT")] public string SFDT { get; set; }
        [JsonProperty("CNMB")] public int CNMB { get; set; }
        [JsonProperty("SCDF")] public string SCDF { get; set; }
        [JsonProperty("SRSC")] public string SRSC { get; set; }
    }
}
