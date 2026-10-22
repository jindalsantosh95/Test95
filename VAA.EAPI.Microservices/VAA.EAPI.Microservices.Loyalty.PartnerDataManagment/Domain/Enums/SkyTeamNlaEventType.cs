using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Enums
{
    public enum SkyTeamNlaEventType
    {
        [EnumMember(Value = "VAA.Loyalty.SkyTeam.InboundAutoAccrualProcessing")]
        InboundAutoAccrualProcessing,

        [EnumMember(Value = "VAA.Loyalty.SkyTeam.InboundAutoAccrualRequest")]
        LoyaltyManagementRequest,

        [EnumMember(Value = "VAA.Loyalty.SkyTeam.InboundAutoAccrualFeedbackGeneration")]
        InboundAutoAccrualFeedbackGeneration
    }
}
