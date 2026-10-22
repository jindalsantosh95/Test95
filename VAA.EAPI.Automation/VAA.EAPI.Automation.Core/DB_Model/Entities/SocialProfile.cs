using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class SocialProfile
    {
        [JsonProperty("DBLastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DbLastUpdatedOn { get; set; }

        [JsonProperty("DBLastUpdatedSrc")]
        public string DbLastUpdatedSrc { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdatedOn { get; set; }

        [JsonProperty("Profile")]
        public string Profile { get; set; }

        [JsonProperty("ProfileValue")]
        public string ProfileValue { get; set; }
    }
}
