using System;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class CustomerProfileDTO : CosmosDocument
    {
        public string CustomerId { get; set; }

        public string DocType { get; set; }

        public Preference[] Preferences { get; set; }

        public string ReasonForTravel { get; set; }

        public string Brand { get; set; }

        public string CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public string LastUpdatedBy { get; set; }

        public string LastUpdatedOn { get; set; }

        public string SourceSystem { get; set; }

        public string SourceIdentifier { get; set; }

        public string SchemaVersion { get; set; } = Constants.Application.SchemaVersion;

        public CustomerProfileDTO(CustomerProfile profile)
        {

            if (profile == null) throw new ArgumentNullException(nameof(profile));

            Id = profile.Id;

            PartitionKey = profile.PartitionKey ?? string.Empty;

            CustomerId = profile.CustomerId ?? string.Empty;

            DocType = profile.DocType ?? string.Empty;

            Preferences = profile.Preferences;

            ReasonForTravel = profile.ReasonForTravel ?? string.Empty;

            Brand = profile.Brand ?? string.Empty;

            CreatedOn = profile.CreatedOn ?? string.Empty;

            CreatedBy = profile.CreatedBy ?? string.Empty;

            LastUpdatedBy = profile.LastUpdatedBy ?? string.Empty;

            LastUpdatedOn = profile.LastUpdatedOn ?? string.Empty;

            SourceSystem = profile.SourceSystem ?? string.Empty;

            SourceIdentifier = profile.SourceIdentifier ?? string.Empty;

        }
    }
}
