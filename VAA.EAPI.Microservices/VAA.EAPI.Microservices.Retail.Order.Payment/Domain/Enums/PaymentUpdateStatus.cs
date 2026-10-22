using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums
{
    public enum PaymentUpdateStatus
    {

        [EnumMember(Value = "Succeeded")]
        Succeeded = 0,
        [EnumMember(Value = "Failed")]
        Failed = 1,
        [EnumMember(Value = "Cancelled")]
        Cancelled = 2
    }
}
