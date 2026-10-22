using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MemberTypes
    {

        [EnumMember(Value = "Individual")]
        Individual = 0,

        [EnumMember(Value = "Household")]
        Household = 1,

        [EnumMember(Value = "Account")]
        Account = 2
    }
}
