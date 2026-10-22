using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums
{
    public enum PaymentEventType
    {
        [EnumMember(Value = "VAA.Order.Payment.InitiateOrderPaymentUpdate")]
        OrderPaymentUpdateInitiate,
        [EnumMember(Value = "VAA.Order.Payment.InitiateOrderPaymentCancel")]
        OrderPaymentCancelInitiate,
        [EnumMember(Value = "VAA.Order.Payment.InitiateVoidOrderUpdate")]
        VoidOrderUpdateInitiate,
        [EnumMember(Value = "VAA.Order.Payment.InitiateCPDErrorCommunication")]
        CPDErrorCommunicationInitiate
    }
}
