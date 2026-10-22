
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain.Enums
{
    public enum EventCommandType
    {
        [EnumMember(Value = "LoyaltyProductReferenceData")]
        LoyaltyProductReferenceData,

        [EnumMember(Value = "LoyaltyPartnerReferenceData")]
        LoyaltyPartnerReferenceData,
    }
}
