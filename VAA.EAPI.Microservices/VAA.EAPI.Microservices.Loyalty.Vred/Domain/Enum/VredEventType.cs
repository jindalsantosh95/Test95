using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Domain.Enum
{
    public enum VredEventType
    {
        [EnumMember(Value = "VAA.Vred.CancelTransaction")]
        CancelTransactionEvent,
        [EnumMember(Value = "VAA.Vred.EarnTransaction")]
        EarnTransactionEvent,
        [EnumMember(Value = "VAA.Vred.BurnTransaction")]
        BurnTransactionEvent,
        [EnumMember(Value = "VAA.Vred.CreateTransaction")]
        CreateTransactionEvent,
        [EnumMember(Value = "VAA.Vred.UpdateVredCustomerDetail")]
        UpdateVredCustomerDetailEvent
    }
}