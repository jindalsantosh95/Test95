using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.NDC.Order.Constants
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActionContextEnum
    {
        [EnumMember(Value = "Delete")]
        Delete = 1,
        [EnumMember(Value = "Add")]
        Add = 2,
        [EnumMember(Value = "Retain")]
        Retain = 3
    }

    public enum DocumentTypeEnum
    {
        T = 1,
        Y = 2,
        INF = 3
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductTypeEnum
    {
        [EnumMember(Value = "Bag")]
        Bag = 1,
        [EnumMember(Value = "Seat")]
        Seat = 2,
        [EnumMember(Value = "Flight")]
        Flight = 3,
        [EnumMember(Value = "SSR")]
        SSREnum = 4
    }

    [JsonConverter(typeof(StringEnumConverter))]
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
        PartiallyCompleted = 5,

        [EnumMember(Value = "Unticketed")]
        Unticketed = 6
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductType
    {
        [EnumMember(Value = "Ticket")]
        Ticket = 100,
        [EnumMember(Value = "Seat")]
        Seat = 200
    }

    public enum CardEnrollStatus
    {
        [EnumMember(Value = "CARDENROLLED3DS1")]
        CardEnrolled3DS1,
        [EnumMember(Value = "CARDENROLLED3DS2")]
        CardEnrolled3DS2,
        [EnumMember(Value = "CARDNOTENROLLED3DS")]
        CardNotEnrolled3DS,
        [EnumMember(Value = "UNKNOWN")]
        Unknown
    }

    public enum CustomerAuthStatus
    {
        [EnumMember(Value = "Y")] //Customer authenticated.
        Y,
        [EnumMember(Value = "N")] //Customer not authenticated.
        N,
        [EnumMember(Value = "A")] //An authentication attempt occurred but could not be completed
        A,
        [EnumMember(Value = "U")] // Unable to perform authentication.
        U,
        [EnumMember(Value = "R")] //  Do not proceed to authorisation
        R
    }

    public enum ContactType
    {
        HOME = 0,
        MOBILE = 1,
        WORK = 2,
        EMAIL = 3
    }

    public enum OrderChangeType
    {
        [EnumMember(Value = "SplitPnr")]
        SplitPnr,
        [EnumMember(Value = "EvenExchange")]
        EvenExchange,
        [EnumMember(Value = "HoldBooking")]
        HoldBooking,
        [EnumMember(Value = "PostSaleSeat")]
        PostSaleSeat,
        [EnumMember(Value = "AddCollect")]
        AddCollect,
        [EnumMember(Value = "Refund")]
        Refund,
        [EnumMember(Value = "Cancel")]
        Cancel,
        [EnumMember(Value = "AmendApis")]
        AmendApis,
        [EnumMember(Value = "Disruption")]
        Disruption,
        [EnumMember(Value = "CancelHoldBooking")]
        CancelHoldBooking
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequestType
    {
        [EnumMember(Value = "OrderCreate")]
        OrderCreate,
        [EnumMember(Value = "OrderChange")]
        OrderChange,
        [EnumMember(Value = "PostSale")]
        PostSale,
        [EnumMember(Value = "OrderCancel")]
        OrderCancel,
        [EnumMember(Value = "EvenExchange")]
        EvenExchange,
        [EnumMember(Value = "HoldBooking")]
        HoldBooking
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ARCRequestType
    {
        [EnumMember(Value = "Rfc")]
        Rfc,
        [EnumMember(Value = "NonRfc")]
        NonRfc,
        [EnumMember(Value = "EvenExchange")]
        EvenExchange,
        [EnumMember(Value = "NameCorrection")]
        NameCorrection,
        [EnumMember(Value = "AddCollect")]
        AddCollect
    }
}