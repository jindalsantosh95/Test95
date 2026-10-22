using Newtonsoft.Json;
using System;
using System.Linq;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class AddressHistory
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string SchemaVersion { get; set; }
        public string DocType { get; set; }
        public string PartitionKey { get; set; }
        public string CustomerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
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

        public static explicit operator AddressHistory(Customers customerDoc)
        {
            Address address = customerDoc.AddressDetails.First(x => x.Primary == "Y");
            return new AddressHistory()
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = customerDoc.PartitionKey,
                CustomerId = customerDoc.CustomerId,
                DocType = DocumentType.AddressHistory.ToString(),
                SourceIdentifier = customerDoc.SourceIdentifier,
                SourceSystem = customerDoc.SourceSystem,
                CreatedBy = customerDoc.CreatedBy,
                CreatedOn = DateTime.Parse(customerDoc.CreatedOn),
                AddressId = address.AddressId,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                AddressLine3 = address.AddressLine3,
                AddressType = address.AddressType,
                PostalStatus = address.PostalStatus,
                City = address.City,
                AdditionalComments = address.AdditionalComments,
                Country = address.Country,
                GeoCodeValidFlag = address.GeoCodeValidFlag,
                LandlordOrCompanyName = address.LandlordOrCompanyName,
                MetroArea = address.MetroArea,
                Valid = address.Valid,
                Province = address.Province,
                GroupAddress = address.GroupAddress,
                PostCode = address.PostCode,
                PartnerFileId = address.PartnerFileId,
                Primary = address.Primary,
                AddressVerificationStatus = address.AddressVerificationStatus,
                AddressVerified = address.AddressVerified,
                LastUpdatedBy = address.LastUpdatedBy,
                LastUpdatedOn = DateTime.Parse(address.LastUpdatedOn),
                SchemaVersion = customerDoc.SchemaVersion

            };
        }
    }
}
