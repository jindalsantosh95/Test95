using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums
{
    public enum CampaignCommandType
    {
        //CampaignManagement - CampaignMgmt (Abbreviation used to keep length below 50char)
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.CampaignRegisterEvent")]
        CampaignRegisterEvent,
        [EnumMember(Value = "VAA.PowerApp.Customer.UpdateTargetedMembership")]
        UpdateTargetedMembership,
        [EnumMember(Value = "VAA.PowerApp.Customer.CampaignEnrolmentDocEvent")]
        CampaignEnrolmentDocEvent,
        [EnumMember(Value = "BasicEmailEvent")]
        CampaignRegisterEmailEvent,
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.UpdateCampaign")]
        UpdateCampaign,
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.CampaignAssignEvent")]
        CampaignAssignEvent,
        [EnumMember(Value = "VAA.PowerApp.Customer.CreateTargetedMembership")]
        CreateTargetedMembership,
        [EnumMember(Value = "VAA.PowerApp.Customer.UpdateFlyingClubMember")]
        UpdateFlyingClubMember,
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.CreateCampaign")]
        CreateCampaign,
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.LinkCampaignPromotion")]
        LinkCampaignPromotion,
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.DelinkCampaignPromotion")]
        DelinkCampaignPromotion,
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.UpdatePromotion")]
        UpdatePromotion,
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.CreatePromotion")]
        CreatePromotion
    }

}
