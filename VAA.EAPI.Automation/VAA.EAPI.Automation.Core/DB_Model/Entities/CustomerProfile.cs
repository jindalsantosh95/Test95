using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{

    public class CustomerProfile
    {
        internal object PreferenceType;

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("Preferences")]
        public Preference[] Preferences { get; set; }

        [JsonProperty("ReasonForTravel")]
        public string ReasonForTravel { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdatedOn { get; set; }

        [JsonProperty("SchemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonProperty("Brand")]
        public string Brand { get; set; }


        [JsonProperty("_etag")]
        public string _etag { get; set; }

    }
}
