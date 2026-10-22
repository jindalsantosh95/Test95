using System.Runtime.Serialization;
namespace VAA.EAPI.Microservices.Loyalty.Vred.Domain.Enum
{
    internal enum VredConsentStatusEnum
    {
        [EnumMember(Value = "CONFIRMED")]
        CONFIRMED,
        [EnumMember(Value = "NOTGIVEN")]
        NOTGIVEN
    }
}
