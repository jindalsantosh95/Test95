using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.MemberProfileUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.MemberProfileUpdate
{
    public static class MemberProfileUpdateMapper
    {
        public static MemberProfileUpdateEventData MaptoMemberProfileEventData(CampaignRegisterEventData campaignDetails)
        {
            if (campaignDetails is null) throw new ArgumentNullException(nameof(campaignDetails));

            MemberProfileUpdateEventData profileData = new MemberProfileUpdateEventData();
            MemberAttributeDocument memberAttributeDoc = new();
            List<MemberAttributeDocument> memberAttributeDocumentList = new();
            profileData.FlyingClubMemberDocument = new() { MemberAttributes = memberAttributeDocumentList };
            profileData.CustomerId = campaignDetails.CustomerId;
            profileData.FlyingClubMemberDocument.LastUpdatedBy = campaignDetails.CreatedBy;

            switch (campaignDetails.CampaignType)
            {
                case Constants.Application.FastTrackToGold:
                    profileData.FlyingClubMemberDocument.FastTrackId = campaignDetails.CampaignId;
                    break;
                case Constants.Application.GoldAfterFirstFlight:
                    profileData.FlyingClubMemberDocument.VAInstantGoldCampaignId = campaignDetails.CampaignId;
                    break;
                case Constants.Application.SilverAfterFirstFlightWithFTG:
                    profileData.FlyingClubMemberDocument.InstantSilverCampaignId = campaignDetails.CampaignId;
                    profileData.FlyingClubMemberDocument.FastTrackId = campaignDetails.CampaignId;
                    break;
                case Constants.Application.SilverAfterFirstFlight:
                    profileData.FlyingClubMemberDocument.InstantSilverCampaignId = campaignDetails.CampaignId;
                    break;
                case Constants.Application.StraightToGold:
                    memberAttributeDoc.AttributeName = Constants.Application.StraightToGoldFlag;
                    memberAttributeDoc.Value = Constants.Application.ProfileFlag;
                    memberAttributeDocumentList.Add(memberAttributeDoc);
                    break;
                case Constants.Application.StraightToSilver:
                    memberAttributeDoc.AttributeName = Constants.Application.StraightToSilverFlag;
                    memberAttributeDoc.Value = Constants.Application.ProfileFlag;
                    memberAttributeDocumentList.Add(memberAttributeDoc);
                    break;
                case Constants.Application.StraightToSilverWithFTG:
                    profileData.FlyingClubMemberDocument.FastTrackId = campaignDetails.CampaignId;
                    memberAttributeDoc.AttributeName = Constants.Application.StraightToSilverFTGFlag;
                    memberAttributeDoc.Value = Constants.Application.ProfileFlag;
                    memberAttributeDocumentList.Add(memberAttributeDoc);
                    break;
            }
            return profileData;
        }

    }
}
