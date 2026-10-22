using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class CampaignEnrollment
    {
        [JsonProperty("SchemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonProperty("Brand")]
        public string Brand { get; set; }

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("LastUpdatedOn")]
        public DateTime LastUpdatedOn { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("MemberNumber")]
        public string MemberNumber { get; set; }

        [JsonProperty("Eligible")]
        public string Eligible { get; set; }

        [JsonProperty("OptInDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime OptInDate { get; set; }

        [JsonProperty("EnrollmentExpiryDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime EnrollmentExpiryDate { get; set; }

        [JsonProperty("EnrollmentId")]
        public string EnrollmentId { get; set; }

        [JsonProperty("PromotionId")]
        public string PromotionId { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

    }
}
