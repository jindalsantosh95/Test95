using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class PartnerMembership
    {
        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("LastUpdatedOn")]
        public DateTime? LastUpdatedOn { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("DBLastUpdatedOn")]
        public string DbLastUpdatedOn { get; set; }

        [JsonProperty("DBLastUpdatedSrc")]
        public string DbLastUpdatedSrc { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("PartnerId")]
        public string PartnerId { get; set; }

        [JsonProperty("ProgramId")]
        public string ProgramId { get; set; }

        [JsonProperty("PartnerStatusCode")]
        public string PartnerStatusCode { get; set; }

        [JsonProperty("StartDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("EndDate")]
        public DateTime? EndDate { get; set; }

        [JsonProperty("PartnerMemberNumber")]
        public string PartnerMemberNumber { get; set; }

        [JsonProperty("PartnerCode")]
        public string PartnerCode { get; set; }

        [JsonProperty("PartnerMembershipMappingId")]
        public string PartnerMembershipMappingId { get; set; }

        [JsonProperty("MemberNumber")]
        public string MemberNumber { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("SchemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonProperty("Brand")]
        public string Brand { get; set; }

    }
}
