using Newtonsoft.Json;
namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class MembershipCard
    {
        [JsonProperty("Active", NullValueHandling = NullValueHandling.Ignore)]
        public string Active { get; set; }

        [JsonProperty("CardExpiryDate", NullValueHandling = NullValueHandling.Ignore)]
        public string? CardExpiryDate { get; set; }

        [JsonProperty("CardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty("CardReplacementReason")]
        public string CardReplacementReason { get; set; }

        [JsonProperty("CardStatusCode")]
        public string CardStatusCode { get; set; }

        [JsonProperty("CardType")]
        public string CardType { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("ExpiryDate", NullValueHandling = NullValueHandling.Ignore)]
        public string? ExpiryDate { get; set; }

        [JsonProperty("IssueDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? IssueDate { get; set; }

        [JsonProperty("MemberNumber")]
        public string MemberNumber { get; set; }

        [JsonProperty("NameOnCard")]
        public string NameOnCard { get; set; }

        [JsonProperty("RequestedDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? RequestedDate { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("StartDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? StartDate { get; set; }

        [JsonProperty("TierCode")]
        public string TierCode { get; set; }

        [JsonProperty("CardID")]
        public string CardID { get; set; }
    }
}
