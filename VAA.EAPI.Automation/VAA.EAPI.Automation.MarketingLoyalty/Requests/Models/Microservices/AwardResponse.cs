using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.Microservices
{
    public class AwardResponse
    {
        [JsonProperty("awards")]
        public List<AwardList> awards { get; set; }

        [JsonProperty("customerId")]
        public string customerId { get; set; }
    }
    public class AwardList
    {
        [JsonProperty("productId")]
        public string productId { get; set; }

        [JsonProperty("startDate")]
        public DateTime startDate { get; set; }

        [JsonProperty("expiryDate")]
        public DateTime? expiryDate { get; set; }

        [JsonProperty("transactionId")]
        public string transactionId { get; set; }

        [JsonProperty("awardBasis")]
        public string awardBasis { get; set; }

        [JsonProperty("awardId")]
        public string awardId { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }

        [JsonProperty("sourceIdentifier")]
        public string sourceIdentifier { get; set; }

        [JsonProperty("sourceSystem")]
        public string sourceSystem { get; set; }

        [JsonProperty("giftedFromCustomerId")]
        public string giftedFromCustomerId { get; set; }

        [JsonProperty("giftedToCustomerId")]
        public string giftedToCustomerId { get; set; }

        [JsonProperty("previousStatus")]
        public string previousStatus { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("createdBy")]
        public string createdBy { get; set; }

        [JsonProperty("createdOn")]
        public DateTime createdOn { get; set; }

        [JsonProperty("lastUpdatedBy")]
        public string lastUpdatedBy { get; set; }

        [JsonProperty("lastUpdatedOn")]
        public DateTime lastUpdatedOn { get; set; }

        [JsonProperty("customerId")]
        public string customerId { get; set; }
    }
}


