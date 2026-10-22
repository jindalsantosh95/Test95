using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI
{
    public class FlightAccrualResponse
    {
        [JsonProperty("code")]
        public string code { get; set; }

        [JsonProperty("message")]
        public string message { get; set; }

        [JsonProperty("customerId")]
        public string customerId { get; set; }

        [JsonProperty("transactionId")]
        public string transactionId { get; set; }
    }
}
