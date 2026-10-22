using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class MemberAttributes
    {
        [JsonProperty("AttributeId", NullValueHandling = NullValueHandling.Ignore)]
        public string AttributeId { get; set; }

        [JsonProperty("SourceIdentifier", NullValueHandling = NullValueHandling.Ignore)]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem", NullValueHandling = NullValueHandling.Ignore)]
        public string SourceSystem { get; set; }

        [JsonProperty("createdOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? createdOn { get; set; }

        [JsonProperty("createdBy")]
        public string createdBy { get; set; }

        [JsonProperty("lastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? lastUpdatedOn { get; set; }

        [JsonProperty("lastUpdatedBy")]
        public string lastUpdatedBy { get; set; }

        [JsonProperty("AttributeName")]
        public string AttributeName { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }


    }
}
