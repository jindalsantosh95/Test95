using Newtonsoft.Json.Converters;
using System;
using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class Email
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum UsageEnum
        {
            Business = 1,
            Personal = 2
        }
        public UsageEnum? Usage { get; set; }

        public Email(UsageEnum? usage = default, string emailAddress = default, string preferredFlag = default, string description = default, string valid = default, DateTime? startDate = default, string primary = default, string emailAddressId = default, DateTime? lastUpdated = default, string lastUpdatedSource = default)
        {
            Usage = usage;
            EmailAddress = emailAddress;
            PreferredFlag = preferredFlag;
            Description = description;
            Valid = valid;
            StartDate = startDate;
            Primary = primary;
            EmailAddressId = emailAddressId;
            LastUpdated = lastUpdated;
            LastUpdatedSource = lastUpdatedSource;
        }

        public string EmailAddress { get; set; }
        public string PreferredFlag { get; set; }
        public string Description { get; set; }
        public string Valid { get; set; }
        public DateTime? StartDate { get; set; }
        public string Primary { get; set; }
        public string EmailAddressId { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string LastUpdatedSource { get; set; }
    }
}
