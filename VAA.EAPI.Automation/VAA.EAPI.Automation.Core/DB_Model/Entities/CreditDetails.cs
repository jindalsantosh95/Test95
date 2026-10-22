using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class CreditDetails
    {
        [JsonProperty("CreditItemId")]
        public string CreditItemId { get; set; }

        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdatedOn { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("CreditValue")]
        public string CreditValue { get; set; }

        [JsonProperty("PromotionRuleActionId")]
        public string PromotionRuleActionId { get; set; }

        [JsonProperty("PointType")]
        public string PointType { get; set; }

        [JsonProperty("Comments")]
        public string Comments { get; set; }

        [JsonProperty("PromotionId")]
        public string PromotionId { get; set; }

        [JsonProperty("PointSubType")]
        public string PointSubType { get; set; }

        [JsonProperty("QualifyingFlag")]
        public string QualifyingFlag { get; set; }

        [JsonProperty("CreditValueUsed")]
        public string CreditValueUsed { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }
    }
}
