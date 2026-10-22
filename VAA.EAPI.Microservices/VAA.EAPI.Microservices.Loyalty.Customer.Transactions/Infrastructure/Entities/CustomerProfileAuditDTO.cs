using System;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class CustomerProfileAuditDTO : CosmosDocument
    {
        public DateTime? DoB { get; set; }
        public string MatchString { get; set; }
        public string FirstName { get; set; }
        public string Title { get; set; }
        public string CustomerId { get; set; }
        public string Gender { get; set; }
        public string MiddleName { get; set; }
        public string EmailAddress { get; set; }
        public string LoyaltyMemberNumber { get; set; }
        public string LastName { get; set; }
        public string DocType { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string SchemaVersion { get; set; }
        public string CustomerProfileUpdatedBy { get; set; }
        public string DowngradeProhibitFlag { get; set; }
        public string SourceIdentifier { get; set; }
        public string Status { get; set; }
        public string NameChangeReason { get; set; }
        public string AuthorisedUser { get; set; }
        public DateTime? CustomerProfileUpdatedOn { get; set; }
        public DateTime? GoldStartDate { get; set; }
        public string TelephonePassword { get; set; }

        public static explicit operator CustomerProfileAuditDTO(PartnerMembership partnerMembership)
        {
            return new()
            {
                Id = partnerMembership.Id,
                PartitionKey = partnerMembership.CustomerId,
                CustomerId = partnerMembership.CustomerId,
                LoyaltyMemberNumber = partnerMembership.MemberNumber ?? string.Empty,
                CustomerProfileUpdatedBy = partnerMembership.LastUpdatedBy ?? string.Empty,
                SourceIdentifier = partnerMembership.SourceIdentifier ?? string.Empty
            };
        }
    }
}
