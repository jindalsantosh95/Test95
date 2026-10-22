using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models
{
    public class TargetedMembershipDTO : CosmosDocument
    {
        public string SchemaVersion { get; set; }
        public string Brand { get; set; }
        public string DocType { get; set; }
        public string CustomerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string AssignmentDate { get; set; }
        public string Comments { get; set; }
        public string CompletedDate { get; set; }
        public string ContactedDate { get; set; }
        public string ProfilingCampaignSourceId { get; set; }
        public string StatusCode { get; set; }
        public string TargetedMembershipId { get; set; }

        public TargetedMembershipDTO()
        {
            // Required for serialisation
        }

    }
}
