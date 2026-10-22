using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class PointOfSale
    {
        [JsonProperty("CityCode")]
        public string CityCode { get; set; }

        [JsonProperty("CountryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("CurrencyCode")]
        public string CurrencyCode { get; set; }
    }
}