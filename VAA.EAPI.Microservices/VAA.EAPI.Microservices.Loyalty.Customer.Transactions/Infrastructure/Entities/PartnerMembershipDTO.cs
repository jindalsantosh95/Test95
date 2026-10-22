using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class PartnerMembershipDTO : CosmosDocument
    {
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string CustomerId { get; set; }
        public string DocType { get; set; }
        public string SchemaVersion { get; set; }
        public string Brand { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string Description { get; set; }
        public string PartnerId { get; set; }
        public string ProgramId { get; set; }
        public string PartnerStatusCode { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string PartnerMemberNumber { get; set; }
        public string PartnerMembershipMappingId { get; set; }
        public string MemberNumber { get; set; }
        public string PartnerCode { get; set; }

        public static explicit operator PartnerMembershipDTO(PartnerMembership partnerMembership)
        {
            return new()
            {
                SchemaVersion = Constants.Application.PartnerMembershipDocSchemaVersion,
                Brand = Constants.Application.PartnerMembershipBrand,
                DocType = ConfigReader.Get(EnvironmentSettings.PartnerMembershipDocType),
                PartitionKey = partnerMembership.CustomerId,
                Id = partnerMembership.Id,
                CreatedBy = partnerMembership.CreatedBy,
                CreatedOn = string.IsNullOrEmpty(partnerMembership.Id) ? DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : partnerMembership.CreatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = partnerMembership.LastUpdatedBy ?? partnerMembership.CreatedBy,
                LastUpdatedOn = DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                CustomerId = partnerMembership.CustomerId,
                SourceIdentifier = partnerMembership.SourceIdentifier ?? string.Empty,
                SourceSystem = partnerMembership.SourceSystem ?? string.Empty,
                PartnerId = partnerMembership.PartnerId ?? string.Empty,
                ProgramId = partnerMembership.ProgramId ?? string.Empty,
                Description = partnerMembership.Description ?? string.Empty,
                PartnerStatusCode = partnerMembership.PartnerStatusCode ?? string.Empty,
                StartDate = partnerMembership.StartDate?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                EndDate = partnerMembership.EndDate?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                PartnerMemberNumber = partnerMembership.PartnerMemberNumber ?? string.Empty,
                PartnerMembershipMappingId = partnerMembership.PartnerMembershipMappingId ?? Utility.GetId(),
                MemberNumber = partnerMembership.MemberNumber ?? string.Empty,
                PartnerCode = partnerMembership.PartnerCode ?? string.Empty
            };
        }
    }
}
