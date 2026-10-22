using System;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CustomerProfile;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class CustomerProfileDTO : CosmosDocument
    {
        public string CustomerId { get; set; }
        public string DocType { get; set; }
        public Preference[] Preferences { get; set; }
        public string ReasonForTravel { get; set; }
        public string Brand { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string SourceSystem { get; set; }
        public string SourceIdentifier { get; set; }
        public string SchemaVersion { get; set; }
    }
}