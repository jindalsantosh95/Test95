using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile
{
    public class Address
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum AddressTypeEnum
        {
            [EnumMember(Value = "Work")]
            Work = 1,
            [EnumMember(Value = "Home")]
            Home = 2
        }

        public AddressTypeEnum? AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string CompanyName { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string PreferredFlag { get; set; }
        public string AddressVerified { get; set; }
    }
}
