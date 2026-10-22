using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class BaseCurrencyAmount
    {
        [JsonProperty("Amount")]
        public string Amount { get; set; }

        [JsonProperty("Currency")]
        public string Currency { get; set; }
    }
}