using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class Address
    {
        public enum AddressTypeEnum
        {
            Work = 1,
            Home = 2,
            Business = 3
        }
        public AddressTypeEnum? AddressType { get; set; }

        public Address(AddressTypeEnum? addressType = default, string addressLine1 = default, string addressLine2 = default, string addressLine3 = default, string companyName = default, string city = default, string postalAddressStatus = default, DateTime? startDate = default, DateTime? created = default, string qasVerified = default, string postalCode = default, string region = default, string territory = default, string country = default, string preferredFlag = default, string valid = default, string primary = default, string addressId = default)
        {
            AddressType = addressType;
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
            AddressLine3 = addressLine3;
            CompanyName = companyName;
            City = city;
            PostalAddressStatus = postalAddressStatus;
            StartDate = startDate;
            Created = created;
            QasVerified = qasVerified;
            PostalCode = postalCode;
            Region = region;
            Territory = territory;
            Country = country;
            PreferredFlag = preferredFlag;
            Valid = valid;
            Primary = primary;
            AddressId = addressId;
        }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string CompanyName { get; set; }
        public string City { get; set; }
        public string PostalAddressStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? Created { get; set; }
        public string QasVerified { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string Territory { get; set; }
        public string Country { get; set; }
        public string PreferredFlag { get; set; }
        public string Valid { get; set; }
        public string Primary { get; set; }
        public string AddressId { get; set; }
    }
}
