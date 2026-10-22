using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CustomerTypeEnum
    {
        [EnumMember(Value = "Basic")]
        Basic = 0,
        [EnumMember(Value = "Loyalty")]
        Loyalty = 1
    }
}
