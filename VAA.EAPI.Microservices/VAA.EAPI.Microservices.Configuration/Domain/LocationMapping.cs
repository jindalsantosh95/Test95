using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Domain
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

        public static explicit operator LocationMapping(LocationMapDbEntity pricingLocationDbEntity)
        {
            return new LocationMapping()
            {
                CountryName = pricingLocationDbEntity.CountryName,
                IATACountryCode = pricingLocationDbEntity.IATACountryCode,
                IATACityCode = pricingLocationDbEntity.IATACityCode,
                CityName = pricingLocationDbEntity.CityName,
                CurrencyCode = pricingLocationDbEntity.CurrencyCode,
                LocationCode = pricingLocationDbEntity.LocationCode,
                ID = Convert.ToInt32(pricingLocationDbEntity.RowKey)
            };
        }
    }
}
