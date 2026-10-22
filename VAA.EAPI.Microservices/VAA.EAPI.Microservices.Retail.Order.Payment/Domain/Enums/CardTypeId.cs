using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CardTypeId
    {
        [EnumMember(Value = "AX")]
        AX = 1,
        [EnumMember(Value = "DC")]
        DC = 3,
        [EnumMember(Value = "TO")]
        TO = 6,
        [EnumMember(Value = "CA")]
        CA = 7,
        [EnumMember(Value = "VI")]
        VI = 8,
        [EnumMember(Value = "TP")]
        TP = 21,
        [EnumMember(Value = "DS")]
        DS = 22
    }
}
