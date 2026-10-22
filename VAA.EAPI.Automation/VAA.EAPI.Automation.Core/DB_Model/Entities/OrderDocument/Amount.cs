using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class Amount
    {
        [JsonProperty("MonetaryAmount")]
        public string MonetaryAmount { get; set; }

        [JsonProperty("BaseCurrency")]
        public string BaseCurrency { get; set; }

        [JsonProperty("DecimalPrecisionCount")]
        public int DecimalPrecisionCount { get; set; }
    }
}