using System.Runtime.Serialization;

namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class OrderAcknowledgement : Document
    {
        public Order Order { get; set; }
        public Link OrderLink { get; set; }
        public string OrderStatus { get; set; }
        public OrderAcknowledgementAlertSecurePayment AlertSecurePayment { get; set; }

        public enum OrderStatusEnum
        {
            [EnumMember(Value = "Open")]
            Open = 1,
            [EnumMember(Value = "Cancelled")]
            Cancelled = 2,
            [EnumMember(Value = "Confirmed")]
            Confirmed = 3,
            [EnumMember(Value = "Completed")]
            Completed = 4,
            [EnumMember(Value = "PartiallyCompleted")]
            PartiallyCompleted = 5
        }
        public enum OrderSubStatusEnum
        {
            [EnumMember(Value = "Pay_Verification")]
            Pay_Verification = 1,
            [EnumMember(Value = "Pay_Verified")]
            Pay_Verified = 2,
            [EnumMember(Value = "Pay_Auth")]
            Pay_Auth = 3,
            [EnumMember(Value = "Pay_Settle")]
            Pay_Settle = 4,
            [EnumMember(Value = "Pay_Unpaid")]
            Pay_Unpaid = 5,

        }

    }
}
