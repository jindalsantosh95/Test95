using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.Enum
{
    public enum ProductTypes
    {
        [EnumMember(Value = "AIRLINE")]
        AIRLINE,
        [EnumMember(Value = "HOTEL")]
        HOTEL,
        [EnumMember(Value = "CAR")]
        CAR
    }
}
