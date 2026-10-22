using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.Enum
{
    public enum DeeplinkOption
    {
        [EnumMember(Value = "FlyReady")]
        FlyReady,
        [EnumMember(Value = "FlightCancel")]
        FlightCancel,
        [EnumMember(Value = "TravelInsurance")]
        TravelInsurance,
        [EnumMember(Value = "IROP_FAF")]
        IROP_FAF,
        [EnumMember(Value = "IROP_ViewDetails")]
        IROP_ViewDetails,
        [EnumMember(Value = "FlightChange")]
        FlightChange,
        [EnumMember(Value = "POPS")]
        POPSMeal,
        [EnumMember(Value = "BidForUpgrade")]
        BidForUpgrade,
        [EnumMember(Value = "BaggageAllowance")]
        BaggageAllowance,
        [EnumMember(Value = "FlightDisruption")]
        FlightDisruption,
        [EnumMember(Value = "ManageHoliday")]
        ManageHoliday,
        [EnumMember(Value = "ConciergeTripAware")]
        ConciergeTripAware
    }
}