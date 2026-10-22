using Newtonsoft.Json;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.Location
{
    public class AirportList
    {
        [JsonProperty(PropertyName = "ID")]
        public int ID { get; set; }

        [JsonProperty(PropertyName = "IATA_AIRPORT_CODE")]
        public string AirportCode { get; set; }

        [JsonProperty(PropertyName = "AIRPORT_NAME")]
        public string AirportName { get; set; }

        [JsonProperty(PropertyName = "LOCATION_MAPPINGID")]
        public string LocationMappingID { get; set; }
    }
}
