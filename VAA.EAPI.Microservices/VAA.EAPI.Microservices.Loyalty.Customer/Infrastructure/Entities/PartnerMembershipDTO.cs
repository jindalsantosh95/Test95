using System;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class PartnerMembershipDTO : CosmosDocument
    {
        public string CustomerId { get; set; }

        public string StartDate { get; set; }

        public string PartnerStatusCode { get; set; }

        public string PartnerMembershipMappingId { get; set; }

        public string CreatedOn { get; set; }

        public string DocType { get; set; }

        public string Brand { get; set; }

        public string SchemaVersion { get; set; } = Constants.Application.SchemaVersion;

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

        public PartnerMembershipDTO()
        {
            //required for seriaization
        }
        public PartnerMembershipDTO(PartnerMembership partnerMembership)
        {
            if (partnerMembership == null) throw new ArgumentNullException(nameof(partnerMembership));


            Id = partnerMembership.Id;
            PartitionKey = partnerMembership.PartitionKey;
            CustomerId = partnerMembership.CustomerId ?? string.Empty;
            StartDate = partnerMembership.StartDate ?? string.Empty;
            PartnerStatusCode = partnerMembership.PartnerStatusCode ?? string.Empty;
            PartnerMembershipMappingId = partnerMembership.PartnerMembershipMappingId ?? string.Empty;
            CreatedOn = partnerMembership.CreatedOn ?? string.Empty;
            DocType = partnerMembership.DocType ?? string.Empty;
            Brand = partnerMembership.Brand ?? string.Empty;
            SchemaVersion = partnerMembership.SchemaVersion ?? string.Empty;
            SourceIdentifier = partnerMembership.SourceIdentifier ?? string.Empty;
            SourceSystem = partnerMembership.SourceSystem ?? string.Empty;
            CreatedBy = partnerMembership.CreatedBy ?? string.Empty;
            LastUpdatedOn = partnerMembership.LastUpdatedOn ?? string.Empty;
            LastUpdatedBy = partnerMembership.LastUpdatedBy ?? string.Empty;
            Description = partnerMembership.Description ?? string.Empty;
            PartnerId = partnerMembership.PartnerId ?? string.Empty;
            ProgramId = partnerMembership.ProgramId ?? string.Empty;
            EndDate = partnerMembership.EndDate ?? string.Empty;
            PartnerMemberNumber = partnerMembership.PartnerMemberNumber ?? string.Empty;
            MemberNumber = partnerMembership.MemberNumber ?? string.Empty;
        }
    }
}
