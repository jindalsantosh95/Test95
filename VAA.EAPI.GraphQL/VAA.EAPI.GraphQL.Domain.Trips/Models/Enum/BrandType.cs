using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Enum
{
    public enum BrandType
    {
        [EnumMember(Value = "VIRGIN_HOLIDAYS")]
        WW = 205,
        [EnumMember(Value = "TRAVEL_CITY_DIRECT")]
        TCD = 200,
        [EnumMember(Value = "VAA")]
        KIOSK = 300,
        [EnumMember(Value = "FLIGHT_ONLY")]
        Fltonly = 400
    }
}