using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public partial class Address
    {
        [JsonProperty("contactType")]
        public string? ContactType { get; set; }

        [JsonProperty("AddressLine1")]
        public string? AddressLine1 { get; set; }

        [JsonProperty("AddressLine2")]
        public string? AddressLine2 { get; set; }

        [JsonProperty("AddressLine3")]
        public string? AddressLine3 { get; set; }

        [JsonProperty("AddressLine4")]
        public string? AddressLine4 { get; set; }

        [JsonProperty("Region")]
        public string? Region { get; set; }

        [JsonProperty("PostalCode")]
        public string? PostalCode { get; set; }

        [JsonProperty("Country")]
        public string? Country { get; set; }

        [JsonProperty("CityName")]
        public string? CityName { get; set; }
    }
}
