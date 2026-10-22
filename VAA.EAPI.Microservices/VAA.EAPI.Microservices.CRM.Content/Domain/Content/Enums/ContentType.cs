using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.CRM.Content.Domain.Content.Enums
{
    public enum ContentType
    {
        [EnumMember(Value = "HOTEL")]
        HOTEL = 0,
        [EnumMember(Value = "CAR")]
        CAR = 1
    }
}
