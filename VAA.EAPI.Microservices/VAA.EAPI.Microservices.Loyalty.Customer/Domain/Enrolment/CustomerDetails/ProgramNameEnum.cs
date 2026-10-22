using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProgramNameEnum
    {

        [EnumMember(Value = "Virgin Atlantic Flying Club")]
        VirginAtlanticFlyingClub = 0
    }
}
