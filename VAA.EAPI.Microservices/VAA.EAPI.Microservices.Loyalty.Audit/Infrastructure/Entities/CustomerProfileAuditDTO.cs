using System;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Entities
{
    public class CustomerProfileAuditDTO : CosmosDocument
    {
        public string DocType { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string CustomerProfileUpdatedBy { get; set; }
        public string CustomerProfileUpdatedOn { get; set; }
        public string CustomerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string DoB { get; set; }
        public string Status { get; set; }
        public string NameChangeReason { get; set; }
        public string AuthorisedUser { get; set; }
        public string DowngradeProhibitFlag { get; set; }
        public string Gender { get; set; }
        public string GoldStartDate { get; set; }
        public string TelephonePassword { get; set; }

        public CustomerProfileAuditDTO(CustomerProfileAudit customerProfileAudit)
        {
            DocType = DocumentType.CustomerProfileAudit.ToString();
            Id = customerProfileAudit?.Id;
            PartitionKey = customerProfileAudit?.CustomerId;
            CustomerId = customerProfileAudit?.CustomerId;
            CreatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat) ?? null;
            CreatedBy = customerProfileAudit?.CustomerProfileUpdatedBy ?? string.Empty;
            CustomerProfileUpdatedOn = customerProfileAudit?.CustomerProfileUpdatedOn?.ToString(Constants.Application.ISODateTimeFormat) ?? null;
            CustomerProfileUpdatedBy = customerProfileAudit?.CustomerProfileUpdatedBy ?? string.Empty;
            SourceIdentifier = customerProfileAudit?.SourceIdentifier ?? string.Empty;
            Title = customerProfileAudit?.Title ?? string.Empty;
            FirstName = customerProfileAudit?.FirstName ?? string.Empty;
            LastName = customerProfileAudit?.LastName ?? string.Empty;
            MiddleName = customerProfileAudit?.MiddleName ?? string.Empty;
            DoB = customerProfileAudit?.DoB?.ToString(Constants.Application.ISODateTimeFormat) ?? null;
            Status = customerProfileAudit?.Status ?? string.Empty;
            NameChangeReason = customerProfileAudit?.NameChangeReason ?? string.Empty;
            AuthorisedUser = customerProfileAudit?.AuthorisedUser ?? string.Empty;
            DowngradeProhibitFlag = customerProfileAudit?.DowngradeProhibitFlag ?? string.Empty;
            Gender = customerProfileAudit?.Gender ?? string.Empty;
            GoldStartDate = customerProfileAudit?.GoldStartDate?.ToString(Constants.Application.ISODateTimeFormat) ?? null;
            TelephonePassword = customerProfileAudit?.TelephonePassword ?? string.Empty;
        }
    }
}