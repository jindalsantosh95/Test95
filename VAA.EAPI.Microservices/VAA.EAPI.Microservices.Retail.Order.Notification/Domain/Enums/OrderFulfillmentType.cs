using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderFulfillmentType
    {
        [EnumMember(Value = "PNR")]
        PNR = 100,
        [EnumMember(Value = "ETicket")]
        ETicket = 101
    }
}
