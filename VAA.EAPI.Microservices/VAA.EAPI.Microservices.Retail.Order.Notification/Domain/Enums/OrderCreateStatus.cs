using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderCreateStatus
    {
        [EnumMember(Value = "Succeeded")]
        Succeeded = 0,
        [EnumMember(Value = "Failed")]
        Failed = 1,
        [EnumMember(Value = "Cancelled")]
        Cancelled = 1
    }
}
