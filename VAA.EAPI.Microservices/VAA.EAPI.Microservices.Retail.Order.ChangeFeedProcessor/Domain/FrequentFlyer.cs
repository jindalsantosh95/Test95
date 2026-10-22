using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public partial class FrequentFlyer
    {
        [JsonProperty("AccountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("AirlineCode")]
        public string AirlineCode { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("OsiNumber")]
        public string OsiNumber { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("Validated")]
        public string Validated { get; set; }
    }
}
