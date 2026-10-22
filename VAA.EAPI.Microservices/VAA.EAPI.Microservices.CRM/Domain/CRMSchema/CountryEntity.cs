using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.CRM.Domain.CRMSchema
{
    public class CountryEntity
    {

        [JsonProperty("vaa_countryid")]
        public string VaaCountryid { get; set; }

        [JsonProperty("_createdby_value")]
        public string CreatedbyValue { get; set; }

        [JsonProperty("vaa_territoryshortname")]
        public string VaaTerritoryshortname { get; set; }

        [JsonProperty("vaa_territorycode")]
        public string VaaTerritorycode { get; set; }

        [JsonProperty("vaa_type")]
        public string VaaType { get; set; }

        [JsonProperty("vaa_name")]
        public string VaaName { get; set; }
        [JsonProperty("_vaa_currency_value")]
        public string VaaCurrencyValue { get; set; }

    }
}
