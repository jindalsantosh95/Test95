using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails
{
    public class Phone
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum PhoneTypes
        {
            [EnumMember(Value = "Landline")]
            Landline = 0,
            [EnumMember(Value = "Mobile")]
            Mobile = 1
        }

        public PhoneTypes? PhoneType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
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
