using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{

    public class ContactDetails
    {
        [JsonProperty("ContactId")]
        public string ContactId { get; set; }

        [JsonProperty("ContactSubType")]
        public string ContactSubType { get; set; }

        [JsonProperty("ContactType")]
        public string ContactType { get; set; }

        [JsonProperty("ContactValue")]
        public string ContactValue { get; set; }

        [JsonProperty("CountryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("CountryWithCode")]
        public string CountryWithCode { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonProperty("DBLastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DbLastUpdatedOn { get; set; }

        [JsonProperty("DBLastUpdatedSrc")]
        public string DbLastUpdatedSrc { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdatedOn { get; set; }

        [JsonProperty("Primary", NullValueHandling = NullValueHandling.Ignore)]
        public string Primary { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("Usage")]
        public string Usage { get; set; }

        [JsonProperty("Valid", NullValueHandling = NullValueHandling.Ignore)]
        public string Valid { get; set; }
    }

}
