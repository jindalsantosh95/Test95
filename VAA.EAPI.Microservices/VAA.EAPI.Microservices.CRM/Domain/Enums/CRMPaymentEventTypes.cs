using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.CRM.Domain.Enums
{
    public enum CRMPaymentEventTypes
    {
        [EnumMember(Value = "VAA.CRM.UpdatePayment")]
        CRMPaymentUpdate
    }
}
