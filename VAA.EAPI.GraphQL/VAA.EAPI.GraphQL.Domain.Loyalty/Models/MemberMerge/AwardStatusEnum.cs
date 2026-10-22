using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberMerge
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AwardStatusEnum
    {
        [EnumMember(Value = "Partially Used")]
        PartiallyUsed = 0,
        [EnumMember(Value = "Available")]
        Available,
        [EnumMember(Value = "Used")]
        Used,
        [EnumMember(Value = "Cancelled")]
        Cancelled,
        Expired,
        Gifted
    }
}
