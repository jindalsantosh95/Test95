using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class RecordG
    {
        [JsonProperty("CNMB")]public int CNMB { get; set; }
        [JsonProperty("FOAD")]public string FOAD { get; set; }
        [JsonProperty("RFIS")]public string RFIS { get; set; }
        [JsonProperty("RFIT")]public string RFIT { get; set; }
        [JsonProperty("CVLU")]public int CVLU { get; set; }
        [JsonProperty("RMRK")]public string RMRK { get; set; }
        [JsonProperty("ICWC")]public int ICWC { get; set; }
        [JsonProperty("ICWD")]public string ICWD { get; set; }
    }

}
