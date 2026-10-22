using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class Record5
    {
        [JsonProperty("TAX_ONE")] public TaxDetail TAX_ONE { get; set; }
        [JsonProperty("TAX_TWO")] public TaxDetail TAX_TWO { get; set; }
        [JsonProperty("TAX_THREE")] public TaxDetail TAX_THREE { get; set; }
        [JsonProperty("TAX_FOUR")] public TaxDetail TAX_FOUR { get; set; }
        [JsonProperty("TAX_FIVE")] public TaxDetail TAX_FIVE { get; set; }
        [JsonProperty("TAX_SIX")] public TaxDetail TAX_SIX { get; set; }
        [JsonProperty("TTDS", DefaultValueHandling = DefaultValueHandling.Include)] public int TTDS { get; set; }
        [JsonProperty("ARIN")] public string ARIN { get; set; }
    }

}
