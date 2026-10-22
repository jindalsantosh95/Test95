using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain.Enums
{
    public enum SeatFulfilmentEventType
    {
        [EnumMember(Value = "VAA.Seat.InitiateSeatFulfilment")]
        SeatFulfilmentInitiate,
        [EnumMember(Value = "VAA.Seat.InitiateOrderCancel")]
        SeatFulfilmentCancelInitiate,
        [EnumMember(Value = "VAA.Seat.ManageCustomer")]
        ManageCustomerInitiate,
        [EnumMember(Value = "VAA.Seat.InitiateSeatDeletion")]
        SeatDeletionInitiate,
        [EnumMember(Value = "VAA.Seat.SeatPaymentReceiptEmailTrigger")]
        SeatPaymentReceiptEmailTrigger,
    }
}
