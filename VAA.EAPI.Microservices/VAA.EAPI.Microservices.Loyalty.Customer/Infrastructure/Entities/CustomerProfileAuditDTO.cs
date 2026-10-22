using Newtonsoft.Json;
using System;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class CustomerProfileAuditDTO : CosmosDocument
    {
        public DateTime? DoB { get; set; }
        public string FirstName { get; set; }
        public string Title { get; set; }
        public string CreatedBy { get; set; }
        public string CustomerId { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Gender { get; set; }
        public string MiddleName { get; set; }
        public string LoyaltyMemberNumber { get; set; }
        public string PartitionKey { get; set; }
        public string LastName { get; set; }
        public string DocType { get; set; }
        public string Status { get; set; }
        public string CustomerProfileUpdatedBy { get; set; }
        public string DowngradeProhibitFlag { get; set; }
        public DateTime CustomerProfileUpdatedOn { get; set; }
        public string SourceIdentifier { get; set; }
        public string NameChangeReason { get; set; }
        public string AuthorisedUser { get; set; }
        public DateTime? GoldStartDate { get; set; }
        public string TelephonePassword { get; set; }

        public static explicit operator CustomerProfileAuditDTO(CustomerProfileAudit audit)
        {
            return new()
            {
                Id = Guid.NewGuid().ToString(),
                DowngradeProhibitFlag = audit.DowngradeProhibitFlag,
                CustomerProfileUpdatedBy = audit.CustomerProfileUpdatedBy,
                CreatedBy = audit.CustomerProfileUpdatedBy,
                PartitionKey = audit.PartitionKey,
                CustomerId = audit.CustomerId,
                LoyaltyMemberNumber = audit.LoyaltyMemberNumber,
                SourceIdentifier = audit.SourceIdentifier,
                TelephonePassword = audit.TelephonePassword,
                GoldStartDate = audit.GoldStartDate
            };
        }
    }
}
