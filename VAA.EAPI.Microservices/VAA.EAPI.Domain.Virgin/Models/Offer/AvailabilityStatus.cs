using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Domain.Virgin.Models.Offer
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AvailabilityStatus
    {
        [EnumMember(Value = "SOLD_OUT")]
        SoldOutEnum = 0,
        [EnumMember(Value = "NOT_OFFERED")]
        NotOfferedEnum = 1,
    }
}
