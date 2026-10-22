using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.CRM.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StatusEnum
    {
        [EnumMember(Value = "Success")]
        Success = 1,
        [EnumMember(Value = "Failure")]
        Failure = 0
    }
}
