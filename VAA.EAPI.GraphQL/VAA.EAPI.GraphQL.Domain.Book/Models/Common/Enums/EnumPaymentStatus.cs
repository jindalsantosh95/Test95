using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EnumPaymentStatus
    {
        [EnumMember(Value = "SUCCESS")]
        SUCCESS = 1,
        [EnumMember(Value = "FAILED")]
        FAILED = 2,
    }
}
