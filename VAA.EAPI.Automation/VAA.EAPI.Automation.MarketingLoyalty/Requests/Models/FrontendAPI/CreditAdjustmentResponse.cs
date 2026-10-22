using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI
{
    public class CreditAdjustmentResponse
    {
        [JsonProperty("code")]
        public int? code { get; set; }

        [JsonProperty("message")]
        public string? message { get; set; }

        [JsonProperty("detail")]
        public string? detail { get; set; }

        [JsonProperty("correlationId")]
        public string? correlationId { get; set; }
    }
}
