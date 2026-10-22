using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order
{
    public enum OrderEventType
    {
        [EnumMember(Value = "VAA.Order.InitiateOrderCreate")]
        OrderCreate,
        [EnumMember(Value = "VAA.Order.InitiateOrderStore")]
        OrderStore,
        [EnumMember(Value = "EnrolmentEventType")]
        CustomerEnrollmentEventType,
        [EnumMember(Value = "VAA.Order.ManageSpecialService")]
        OrderSsr,
        [EnumMember(Value = "VAA.Order.AddSpecialRemarks")]
        OrderSpecialRemarks,
        [EnumMember(Value = "VAA.Order.InitiateInsurancePurchase")]
        InsurancePurchase,
        [EnumMember(Value = "VAA.Order.InitiateInsuranceDecision")]
        InsuranceDecision,
        [EnumMember(Value = "VAA.Order.InitiateClearPaymentDetails")]
        ClearPaymentDetails
    }
}
