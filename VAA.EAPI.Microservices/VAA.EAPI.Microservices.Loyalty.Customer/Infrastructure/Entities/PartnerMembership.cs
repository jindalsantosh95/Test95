using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class PartnerMembership
    {
        public string CustomerId { get; set; }

        public string StartDate { get; set; }

        public string PartnerStatusCode { get; set; }

        public string PartnerMembershipMappingId { get; set; }

        public string CreatedOn { get; set; }

        public string DocType { get; set; }

        public string PartitionKey { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Brand { get; set; }

        public string SchemaVersion { get; set; }

        public string SourceIdentifier { get; set; }

        public string SourceSystem { get; set; }

        public string CreatedBy { get; set; }

        public string LastUpdatedOn { get; set; }

        public string LastUpdatedBy { get; set; }

        public string Description { get; set; }

        public string PartnerId { get; set; }

        public string ProgramId { get; set; }

        public string EndDate { get; set; }

        public string PartnerMemberNumber { get; set; }

        public string MemberNumber { get; set; }
    }
}
