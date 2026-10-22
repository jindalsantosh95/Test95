using Newtonsoft.Json;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.Location
{
    public class LocationMapping
    {
        [JsonProperty(PropertyName = "IATA_COUNTRY_CODE")]
        public string IATACountryCode { get; set; }

        [JsonProperty(PropertyName = "COUNTRY_NAME")]
        public string CountryName { get; set; }

        [JsonProperty(PropertyName = "IATA_CITY_CODE")]
        public string IATACityCode { get; set; }

        [JsonProperty(PropertyName = "CITY_NAME")]
        public string CityName { get; set; }

        [JsonProperty(PropertyName = "CURRENCY_CODE")]
        public string CurrencyCode { get; set; }

        [JsonProperty(PropertyName = "LOCATION_CODE")]
        public string LocationCode { get; set; }

        [JsonProperty(PropertyName = "ID")]
        public int ID { get; set; }
    }
}
