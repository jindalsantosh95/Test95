using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CabinClass
    {
        [EnumMember(Value = "Upper Class")]
        C,
        [EnumMember(Value = "Premium")]
        W,
        [EnumMember(Value = "Economy")]
        Y
    }
}