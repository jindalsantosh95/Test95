using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.Enums
{
    public enum OrderEventType
    {
        [EnumMember(Value = "VAA.Order.InitiateOrderChange")]
        OrderChangeInitiate,
        [EnumMember(Value = "VAA.Order.InitiateOrderCancel")]
        OrderCancelInitiate,
        [EnumMember(Value = "VAA.Order.ManageCustomer")]
        ManageCustomerInitiate,
        [EnumMember(Value = "VAA.Order.ManageSpecialService")]
        ManageSpecialServiceInitiate,
        [EnumMember(Value = "VAA.Order.SplitPnrInitiate")]
        SplitPnrInitiate,
    }
}
