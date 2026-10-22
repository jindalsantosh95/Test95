using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI
{
    public class VredStatement
    {
        [JsonProperty("statements")]
        public List<Statement> statements { get; set; }

        [JsonProperty("limit")]
        public int limit { get; set; }

        [JsonProperty("offset")]
        public int offset { get; set; }

        [JsonProperty("total")]
        public int total { get; set; }
    }

    public class Statement
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("statementLineId")]
        public string statementLineId { get; set; }

        [JsonProperty("activityAt")]
        public string activityAt { get; set; }

        [JsonProperty("transactionAt")]
        public string transactionAt { get; set; }

        [JsonProperty("activityDate")]
        public DateTime activityDate { get; set; }

        [JsonProperty("transactionDate")]
        public DateTime transactionDate { get; set; }

        [JsonProperty("description")]
        public string description { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("points")]
        public int points { get; set; }

        [JsonProperty("pointsInt")]
        public int pointsInt { get; set; }

        [JsonProperty("details")]
        public string details { get; set; }
    }
}
