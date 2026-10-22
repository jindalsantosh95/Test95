using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequestOptionCodeType
    {
        [EnumMember(Value = "PNR")]
        PNR = 1,
        [EnumMember(Value = "TICKET")]
        TICKET = 2,
        [EnumMember(Value = "CREDITCARD")]
        CREDITCARD = 3,
        [EnumMember(Value = "LOYALTYNUM")]
        LOYALTYNUM = 4,
        [EnumMember(Value = "HOLIDAYS")]
        HOLIDAYS = 5
    }
}
