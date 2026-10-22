using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderNotificationTypes
    {
        [EnumMember(Value = "VAA.Order.Notification.InitiateOrderCreate")]
        OrderNotificationInitiate = 0,
        [EnumMember(Value = "VAA.Order.Data.OrderNotificationCreateStatus")]
        OrderNotificationStatus = 1

    }
}
