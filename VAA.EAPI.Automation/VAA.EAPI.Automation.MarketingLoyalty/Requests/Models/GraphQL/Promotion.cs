using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL
{
    public class Promotion
    {
        [JsonProperty("PromotionId")]
        public string PromotionId { get; set; }

        [JsonProperty("PromotionName")]
        public string PromotionName { get; set; }

        [JsonProperty("StartDate")]
        public string StartDate { get; set; }

        [JsonProperty("EndDate")]
        public string EndDate { get; set; }

        [JsonProperty("PartnerName")]
        public string PartnerName { get; set; }

        [JsonProperty("LastModifiedBy")]
        public string LastModifiedBy { get; set; }
    }
}
