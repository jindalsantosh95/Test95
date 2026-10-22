using Newtonsoft.Json.Converters;
using System;
using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class Phone
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ContactSubTypeEnum
        {
            Landline = 1,
            Mobile = 2
        }

        public ContactSubTypeEnum? ContactSubType { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public enum UsageEnum
        {
            Business = 1,
            Personal = 2
        }

        public UsageEnum? Usage { get; set; }

        public Phone(ContactSubTypeEnum? contactSubType = default, UsageEnum? usage = default, string countryCode = default, string phoneNumber = default, string preferredFlag = default, string description = default, string valid = default, string primary = default, DateTime? startDate = default, string phoneId = default)
        {
            ContactSubType = contactSubType;
            Usage = usage;
            CountryCode = countryCode;
            PhoneNumber = phoneNumber;
            PreferredFlag = preferredFlag;
            Description = description;
            Valid = valid;
            Primary = primary;
            StartDate = startDate;
            PhoneId = phoneId;
        }

        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public string PreferredFlag { get; set; }
        public string Description { get; set; }
        public string Valid { get; set; }
        public string Primary { get; set; }
        public DateTime? StartDate { get; set; }
        public string PhoneId { get; set; }
    }
}
