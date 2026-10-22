using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Enum
{
    public enum ContentType
    {
        [EnumMember(Value = "HOTEL")]
        HOTEL = 0,
        [EnumMember(Value = "CAR")]
        CAR = 1
    }
}