using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership
{
    public class PartnerMembership : CustomerTransaction
    {
        public string DocType { get; set; }
        public string SchemaVersion { get; set; }
        public string Brand { get; set; }
        public string PartitionKey { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string Description { get; set; }
        public string PartnerId { get; set; }
        public string ProgramId { get; set; }
        public string PartnerStatusCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PartnerMemberNumber { get; set; }
        public string PartnerMembershipMappingId { get; set; }
        public string MemberNumber { get; set; }
        public string PartnerCode { get; set; }

        public static explicit operator PartnerMembership(PartnerMembershipDTO partnerMembershipDto) => new()
        {
            Id = partnerMembershipDto.Id,
            CustomerId = partnerMembershipDto.CustomerId,
            CreatedOn = DateTime.Parse(partnerMembershipDto.CreatedOn),
            CreatedBy = partnerMembershipDto.CreatedBy,
            LastUpdatedOn = DateTime.Parse(partnerMembershipDto.LastUpdatedOn),
            LastUpdatedBy = partnerMembershipDto.LastUpdatedBy,
            SourceIdentifier = partnerMembershipDto.SourceIdentifier,
            SourceSystem = partnerMembershipDto.SourceSystem,
            Brand = partnerMembershipDto.Brand,
            PartnerId = partnerMembershipDto.PartnerId,
            ProgramId = partnerMembershipDto.ProgramId,
            PartnerStatusCode = partnerMembershipDto.PartnerStatusCode,
            StartDate = !string.IsNullOrEmpty(partnerMembershipDto.StartDate) ? DateTime.Parse(partnerMembershipDto.StartDate) : null,
            EndDate = !string.IsNullOrEmpty(partnerMembershipDto.EndDate) ? DateTime.Parse(partnerMembershipDto.EndDate) : null,
            Description = partnerMembershipDto.Description,
            PartnerMemberNumber = partnerMembershipDto.PartnerMemberNumber,
            PartnerMembershipMappingId = partnerMembershipDto.PartnerMembershipMappingId,
            MemberNumber = partnerMembershipDto.MemberNumber,
            PartnerCode = partnerMembershipDto.PartnerCode
        };
    }
}
