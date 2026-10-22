using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities
{
    public class AddressDetail
    {
        public enum AddressTypes
        {

            [EnumMember(Value = "Work")]
            Work = 0,
            [EnumMember(Value = "Home")]
            Home = 1,
            [EnumMember(Value = "Business")]
            Business = 2
        }

        public AddressTypes? AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string CompanyName { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string PreferredFlag { get; set; }
        public bool AddressVerified { get; set; }
        public string Valid { get; set; }
        public string MetroArea { get; set; }
    }
}
