using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class PriceDetails
    {
        [JsonProperty("PriceComponentType")]
        public string PriceComponentType { get; set; }

        [JsonProperty("Amount")]
        public Amount Amount { get; set; }

        [JsonProperty("BaseCurrencyAmount")]
        public BaseCurrencyAmount BaseCurrencyAmount { get; set; }
    }
}