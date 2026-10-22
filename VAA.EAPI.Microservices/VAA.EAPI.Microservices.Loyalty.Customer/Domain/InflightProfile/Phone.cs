using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile
{
    public class Phone
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum PhoneTypeEnum
        {
            [EnumMember(Value = "Landline")]
            Landline = 1,
            [EnumMember(Value = "Mobile")]
            Mobile = 2
        }

        public PhoneTypeEnum? PhoneType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum UsageEnum
        {
            [EnumMember(Value = "Business")]
            Business = 1,
            [EnumMember(Value = "Personal")]
            Personal = 2
        }

        public UsageEnum? Usage { get; set; }

        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public string PreferredFlag { get; set; }
        public string Description { get; set; }
    }
}
