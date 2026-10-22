using System;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class AddressHistoryDTO : CosmosDocument
    {
        public string SchemaVersion { get; set; }
        public string DocType { get; set; }
        public string CustomerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string AddressId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string PostalStatus { get; set; }
        public string AddressType { get; set; }
        public string City { get; set; }
        public string AdditionalComments { get; set; }
        public string Country { get; set; }
        public string GeoCodeValidFlag { get; set; }
        public string LandlordOrCompanyName { get; set; }
        public string MetroArea { get; set; }
        public string Valid { get; set; }
        public string Province { get; set; }
        public string AddressVerified { get; set; }
        public string AddressVerificationStatus { get; set; }
        public string PostCode { get; set; }
        public string GroupAddress { get; set; }
        public string PartnerFileId { get; set; }
        public string Primary { get; set; }

        public AddressHistoryDTO()
        {

        }

        public AddressHistoryDTO(AddressHistory addressHistory)
        {
            if (addressHistory == null) throw new ArgumentNullException(nameof(addressHistory));

            Id = addressHistory.Id;
            PartitionKey = addressHistory.PartitionKey;
            SchemaVersion = Constants.Application.SchemaVersion;
            DocType = addressHistory.DocType;
            CustomerId = addressHistory.CustomerId;
            SourceIdentifier = addressHistory.SourceIdentifier;
            SourceSystem = addressHistory.SourceSystem;
            CreatedOn = addressHistory.CreatedOn?.ToString(Constants.Application.ISODateTimeFormat) ?? DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat);
            CreatedBy = addressHistory.CreatedBy;
            LastUpdatedOn = addressHistory.LastUpdatedOn?.ToString(Constants.Application.ISODateTimeFormat) ?? DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat);
            LastUpdatedBy = addressHistory.LastUpdatedBy;
            AddressId = addressHistory.AddressId ?? string.Empty;
            AddressLine1 = addressHistory.AddressLine1 ?? string.Empty;
            AddressLine2 = addressHistory.AddressLine2 ?? string.Empty;
            AddressLine3 = addressHistory.AddressLine3 ?? string.Empty;
            PostalStatus = addressHistory.PostalStatus ?? string.Empty;
            AddressType = addressHistory.AddressType ?? string.Empty;
            City = addressHistory.City ?? string.Empty;
            AdditionalComments = addressHistory.AdditionalComments ?? string.Empty;
            Country = addressHistory.Country ?? string.Empty;
            GeoCodeValidFlag = addressHistory.GeoCodeValidFlag ?? string.Empty;
            LandlordOrCompanyName = addressHistory.LandlordOrCompanyName ?? string.Empty;
            MetroArea = addressHistory.MetroArea ?? string.Empty;
            Valid = addressHistory.Valid ?? string.Empty;
            Province = addressHistory.Province ?? string.Empty;
            AddressVerified = addressHistory.AddressVerified ?? string.Empty;
            AddressVerificationStatus = addressHistory.AddressVerificationStatus ?? string.Empty;
            PostCode = addressHistory.PostCode ?? string.Empty;
            GroupAddress = addressHistory.GroupAddress ?? string.Empty;
            PartnerFileId = addressHistory.PartnerFileId ?? string.Empty;
            Primary = addressHistory.Primary ?? string.Empty;
        }
    }
}
