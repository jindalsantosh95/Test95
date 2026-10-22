using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.CRM.Email.Constants
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequestType
    {
        [EnumMember(Value = "OrderCreate")]
        OrderCreate,
        [EnumMember(Value = "OrderChange")]
        OrderChange,
        [EnumMember(Value = "PostSale")]
        PostSale,
        [EnumMember(Value = "OrderCancel")]
        OrderCancel,
        [EnumMember(Value = "EvenExchange")]
        EvenExchange,
        [EnumMember(Value = "HoldBooking")]
        HoldBooking

    }
}
