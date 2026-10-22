using Newtonsoft.Json;
namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{

    public class Group
    {

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("Brand")]
        public string Brand { get; set; }

        [JsonProperty("SchemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("GroupId")]
        public string GroupId { get; set; }

        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public string CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public string LastUpdatedOn { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("GroupName")]
        public string GroupName { get; set; }

        [JsonProperty("GroupTypeCode")]
        public string GroupTypeCode { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("HeadOfGroupCustomerId")]
        public string HeadOfGroupCustomerId { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("GroupHistory")]
        public GroupHistory[] GroupHistory { get; set; }

        [JsonProperty("_etag")]
        public string _etag { get; set; }

    }
}