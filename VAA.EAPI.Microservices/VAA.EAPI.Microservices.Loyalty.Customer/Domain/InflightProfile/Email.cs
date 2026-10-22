using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile
{
    public class Email
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum UsageEnum
        {
            [EnumMember(Value = "Business")]
            Business = 1,
            [EnumMember(Value = "Personal")]
            Personal = 2
        }
        public UsageEnum? Usage { get; set; }

        public string EmailId { get; set; }
        public string PreferredFlag { get; set; }
        public string Description { get; set; }
        public string Valid { get; set; }
    }
}
