using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class CardMethod
    {
        [JsonProperty("CardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty("ExpiryDate")]
        public string ExpiryDate { get; set; }

        [JsonProperty("CardholderName")]
        public string CardholderName { get; set; }

        [JsonProperty("Cvv")]
        public string Cvv { get; set; }

        [JsonProperty("BillingAddress")]
        public BillingAddress BillingAddress { get; set; }

        [JsonProperty("CardNetworkCode")]
        public string CardNetworkCode { get; set; }
    }
}