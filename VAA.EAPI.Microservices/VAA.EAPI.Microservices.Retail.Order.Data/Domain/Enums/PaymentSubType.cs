using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums
{
    public enum PaymentSubType
    {
        [EnumMember(Value = "Credit")]
        Credit = 1,
        [EnumMember(Value = "Debit")]
        Debit = 2
    }
}
