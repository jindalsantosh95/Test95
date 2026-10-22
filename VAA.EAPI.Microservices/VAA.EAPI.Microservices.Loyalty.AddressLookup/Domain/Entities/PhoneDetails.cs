using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities
{
    public class PhoneDetails
    {
        public enum PhoneTypes
        {
            [EnumMember(Value = "Landline")]
            Landline = 0,
            [EnumMember(Value = "Mobile")]
            Mobile = 1
        }

        public PhoneTypes? PhoneType { get; set; }
        public enum Usages
        {
            [EnumMember(Value = "Business")]
            Business = 0,
            [EnumMember(Value = "Personal")]
            Personal = 1
        }

        public Usages? Usage { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public string PreferredFlag { get; set; }
        public string Description { get; set; }
    }
}