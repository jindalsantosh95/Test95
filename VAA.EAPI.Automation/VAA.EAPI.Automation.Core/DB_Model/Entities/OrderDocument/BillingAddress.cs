using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class BillingAddress
    {
        [JsonProperty("AddressLine1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("City")]
        public string City { get; set; }

        [JsonProperty("PostalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("Region")]
        public string Region { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }
    }
}