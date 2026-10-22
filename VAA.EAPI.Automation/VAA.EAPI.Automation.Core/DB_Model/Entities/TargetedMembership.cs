using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class TargetedMembership
    {
        [JsonProperty("SchemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("Brand")]
        public string Brand { get; set; }

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("TargetedMembershipId")]
        public string TargetedMembershipId { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdatedOn { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("AssignmentDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? AssignmentDate { get; set; }

        [JsonProperty("Comments")]
        public string Comments { get; set; }

        [JsonProperty("CompletedDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CompletedDate { get; set; }

        [JsonProperty("ContactedDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ContactedDate { get; set; }

        [JsonProperty("ProfilingCampaignSourceId")]
        public string ProfilingCampaignSourceId { get; set; }

        [JsonProperty("StatusCode")]
        public string StatusCode { get; set; }

        [JsonProperty("_etag")]
        public string _etag { get; set; }
    }
}
