using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums
{
    public enum CampaignEventType
    {
        //CampaignManagement - CampaignMgmt (Abbreviation used to keep length below 50char)
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.UpdateCampaign")]
        CampaignManagementUpdateCampaign,
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.CampaignRegisterEvent")]
        CampaignRegisterInitiate,
        [EnumMember(Value = "VAA.PowerApp.Customer.UpdateTargetedMembership")]
        TargetedMembershipUpdateEventInitiate,
        [EnumMember(Value = "BasicEmailEvent")]
        CampaignRegisterEmailEvent,
        [EnumMember(Value = "VAA.PowerApp.Customer.CampaignEnrolmentDocEvent")]
        CampaignEnrolmentDocEventInitiate,
        [EnumMember(Value = "VAA.PowerApp.Customer.CreateTargetedMembership")]
        TargetedMembershipCreateEvent,
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.CampaignAssignEvent")]
        CampaignAssignInitiate,
        [EnumMember(Value = "VAA.PowerApp.Customer.UpdateFlyingClubMember")]
        MemberProfileUpdateEventInitiate,
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.CreateCampaign")]
        CampaignManagementCreateCampaign,
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.LinkCampaignPromotion")]
        LinkCampaignPromotion,
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.DelinkCampaignPromotion")]
        DelinkCampaignPromotion,
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.UpdatePromotion")]
        CampaignManagementUpdatePromotion,
        [EnumMember(Value = "VAA.PowerApp.Customer.DeleteTargetedMembership")]
        TargetedMembershipDeleteEvent,
        [EnumMember(Value = "VAA.PowerApp.CampaignMgmt.CreatePromotion")]
        CampaignManagementCreatePromotion
    }
}
