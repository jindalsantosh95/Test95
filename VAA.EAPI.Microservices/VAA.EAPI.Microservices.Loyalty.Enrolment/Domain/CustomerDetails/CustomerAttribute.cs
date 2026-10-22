using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails
{
    public class CustomerAttribute
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Types
        {
            [EnumMember(Value = "IphoneApp")]
            IphoneApp = 0,
            [EnumMember(Value = "TwitterHandle")]
            TwitterHandle = 1,
            [EnumMember(Value = "VAFacebookApp")]
            VAFacebookApp = 2
        }

        public Types? Type { get; set; }
        public string Value { get; set; }


    }
}
