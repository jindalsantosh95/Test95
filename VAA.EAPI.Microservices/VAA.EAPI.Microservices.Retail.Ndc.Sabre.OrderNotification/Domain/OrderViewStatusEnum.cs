using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderViewStatusEnum
    {
        [EnumMember(Value = "ACTIVE")]
        ACTIVE = 0,
        [EnumMember(Value = "OPENED")]
        OPENED = 1,
        [EnumMember(Value = "CLOSED")]
        CLOSED = 2,
        [EnumMember(Value = "CANCELLED")]
        CANCELLED = 3,
        [EnumMember(Value = "REQUESTED")]
        REQUESTED = 4,
        [EnumMember(Value = "CONFIRMED")]
        CONFIRMED = 5,
        [EnumMember(Value = "DELIVERED")]
        DELIVERED = 6,
        [EnumMember(Value = "REMOVED")]
        REMOVED = 7, 
        [EnumMember(Value = "WAITLISTED")]
        WAITLISTED = 8, 
        [EnumMember(Value = "READY TO PROCEED")]
        READYTOPROCEED = 9, 
        [EnumMember(Value = "READY TO DELIVER")]
        READYTODELIVER = 10,
    }
}
