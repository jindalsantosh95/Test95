using System.Runtime.Serialization;
namespace VAA.EAPI.Microservices.Retail.Order.Domain.Enums
{
    public enum OrderEventType
    {
        [EnumMember(Value = "VAA.Order.InitiateOrderChange")]
        OrderChangeInitiate,
        [EnumMember(Value = "VAA.Order.InitiateOrderCancel")]
        OrderCancelInitiate,
    }
}
