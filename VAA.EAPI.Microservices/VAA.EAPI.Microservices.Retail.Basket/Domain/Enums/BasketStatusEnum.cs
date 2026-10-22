using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BasketStatusEnum
    {
        [EnumMember(Value = "CREATED")]
        CREATED,

        [EnumMember(Value = "PROCESSING")]
        PROCESSING,

        [EnumMember(Value = "RETRY")]
        RETRY,

        [EnumMember(Value = "FAILED")]
        FAILED,

        [EnumMember(Value = "COMPLETED")]
        COMPLETED
    }
}

