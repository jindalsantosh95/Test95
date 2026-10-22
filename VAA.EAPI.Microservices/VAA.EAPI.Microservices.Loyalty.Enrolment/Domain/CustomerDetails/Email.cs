using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails
{

    public class Email
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Usages
        {
            [EnumMember(Value = "Business")]
            Business = 0,
            [EnumMember(Value = "Personal")]
            Personal = 1
        }

        public Usages? Usage { get; set; }
        public string EmailId { get; set; }
        public string PreferredFlag { get; set; }
        public string Description { get; set; }

    }
}
