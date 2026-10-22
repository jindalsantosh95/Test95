using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CustomerProfile
{
    public class CustomerProfile
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
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

        public CustomerProfile(CustomerProfileDTO profileDto)
        {
            if (profileDto == null) throw new ArgumentNullException(nameof(profileDto));

            Id = profileDto.Id;
            CustomerId = profileDto.CustomerId;
            Preferences = profileDto.Preferences;
            ReasonForTravel = profileDto.ReasonForTravel;
            Brand = profileDto.Brand;
            CreatedOn = profileDto.CreatedOn;
            CreatedBy = profileDto.CreatedBy;
            LastUpdatedBy = profileDto.LastUpdatedBy;
            LastUpdatedOn = profileDto.LastUpdatedOn;
            SourceSystem = profileDto.SourceSystem;
            SourceIdentifier = profileDto.SourceIdentifier;
            SchemaVersion = profileDto.SchemaVersion;
        }
    }
}