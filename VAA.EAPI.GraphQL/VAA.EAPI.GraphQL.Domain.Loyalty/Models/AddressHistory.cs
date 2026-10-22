using System;
using System.Linq;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class AddressHistory
    {
        public string CustomerId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
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

        public static explicit operator AddressHistory(Customers source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            var presentAddress = source?.AddressDetails.FirstOrDefault(a => a.Primary == "Y");

            return new AddressHistory
            {
                AdditionalComments = presentAddress?.AdditionalComments,
                AddressId = presentAddress?.AddressId,
                AddressLine1 = presentAddress?.AddressLine1,
                AddressLine2 = presentAddress?.AddressLine2,
                AddressLine3 = presentAddress?.AddressLine3,
                AddressType = presentAddress?.AddressType,
                AddressVerificationStatus = presentAddress?.AddressVerificationStatus,
                AddressVerified = presentAddress?.AddressVerified,
                City = presentAddress?.City,
                Country = presentAddress?.Country,
                CreatedBy = presentAddress?.CreatedBy,
                CreatedOn = (DateTime)(presentAddress?.LastUpdatedOn),
                CustomerId = source?.CustomerId,
                GeoCodeValidFlag = presentAddress?.GeoCodeValidFlag,
                GroupAddress = presentAddress?.GroupAddress,
                LandlordOrCompanyName = presentAddress?.LandlordOrCompanyName,
                LastUpdatedBy = presentAddress?.LastUpdatedBy,
                LastUpdatedOn = (DateTime)(presentAddress?.LastUpdatedOn),
                MetroArea = presentAddress?.MetroArea,
                PartnerFileId = presentAddress?.PartnerFileId,
                PostalStatus = presentAddress?.PostalStatus,
                PostCode = presentAddress?.PostCode,
                Primary = presentAddress?.Primary,
                Province = presentAddress?.Province,
                Valid = presentAddress?.Valid
            };
        }
    }
}


