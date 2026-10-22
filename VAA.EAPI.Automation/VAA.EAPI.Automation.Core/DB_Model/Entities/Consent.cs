
using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class Consent
    {
        [JsonProperty("ConsentFor")]
        public string ConsentFor { get; set; }

        [JsonProperty("ConsentProcessedDate")]
        public string ConsentProcessedDate { get; set; }

        [JsonProperty("DBLastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DbLastUpdatedOn { get; set; }

        [JsonProperty("DBLastUpdatedSrc")]
        public string DbLastUpdatedSrc { get; set; }

        [JsonProperty("EffectiveFrom", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? EffectiveFrom { get; set; }

        [JsonProperty("EffectiveTo", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? EffectiveTo { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdatedOn { get; set; }

        [JsonProperty("TermsAndConditionsAccepted")]
        public string TermsAndConditionsAccepted { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedOn { get; set; }


    }
}
