using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BasketStatusEnum
    {
        [EnumMember(Value = "CREATED")]
        CREATED,

        [EnumMember(Value = "PROCESSING")]
        PROCESSING,

        [EnumMember(Value = "FAILED")]
        FAILED,

        [EnumMember(Value = "RETRY")]
        RETRY
    }
}