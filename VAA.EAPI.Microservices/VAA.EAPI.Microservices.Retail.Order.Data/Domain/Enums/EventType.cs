using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums
{
    public enum EventType
    {
        [EnumMember(Value = "VAA.Order.Data.OrderPaymentUpdateStatus")]
        OrderPaymentUpdateStatus,
        [EnumMember(Value = "VAA.Order.Data.GetVoidTicketOrderDetail")]
        OrderReadEventType,
        [EnumMember(Value = "VAA.Order.Data.OrderNotificationCreateStatus")]
        OrderNotificationCreateStatusEventType
    }
}
