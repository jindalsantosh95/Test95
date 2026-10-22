using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberMerge
{
    public class FlyingClubMemberRequest
    {
        public FlyingClubMember.FlyingClubMember FlyingClubMemberDocument { get; set; }

        public static explicit operator FlyingClubMemberRequest(FlyingClubMember.FlyingClubMember flyingClubMemberDoc)
        {
            if (flyingClubMemberDoc is null) throw new ArgumentNullException(nameof(flyingClubMemberDoc));

            return new FlyingClubMemberRequest()
            {
                FlyingClubMemberDocument = new FlyingClubMember.FlyingClubMember
                {
                    CreatedBy = flyingClubMemberDoc.CreatedBy,
                    CreatedOn = flyingClubMemberDoc.CreatedOn,
                    CustomerId = flyingClubMemberDoc.CustomerId,
                    MemberAttributes = flyingClubMemberDoc.MemberAttributes,
                    SourceSystem = flyingClubMemberDoc.SourceSystem,
                    AutoAccrualStartDate = flyingClubMemberDoc.AutoAccrualStartDate,
                    GoldStartDate = flyingClubMemberDoc.GoldStartDate,
                    RetainSilvercounter = flyingClubMemberDoc.RetainSilvercounter,
                    CorporateMembershipNumber = flyingClubMemberDoc.CorporateMembershipNumber,
                    RetainGoldCounter = flyingClubMemberDoc.RetainGoldCounter,
                    LastAccrualTxnProcessDate = flyingClubMemberDoc.LastAccrualTxnProcessDate,
                    LastRedemptionTxnProcessedDate = flyingClubMemberDoc.LastRedemptionTxnProcessedDate,
                    LastTransactionProcessedDate = flyingClubMemberDoc.LastTransactionProcessedDate,
                    LoungeNumber = flyingClubMemberDoc.LoungeNumber,
                    MemberNumber = flyingClubMemberDoc.MemberNumber,
                    MemberType = flyingClubMemberDoc.MemberType,
                    GroupId = flyingClubMemberDoc.GroupId,
                    HeadOfGroupMemberNumber = flyingClubMemberDoc.HeadOfGroupMemberNumber,
                    TotalTierPoints = flyingClubMemberDoc.TotalTierPoints,
                    VSOnlyTotalTierPoints = flyingClubMemberDoc.VSOnlyTotalTierPoints,
                    VSFlownSectors = flyingClubMemberDoc.VSFlownSectors,
                    ProgramId = flyingClubMemberDoc.ProgramId,
                    TierCode = flyingClubMemberDoc.TierCode,
                    EnrollmentDate = flyingClubMemberDoc.EnrollmentDate,
                    TestMemberFlag = flyingClubMemberDoc.TestMemberFlag,
                    TermsAndConditionsFlag = flyingClubMemberDoc.TermsAndConditionsFlag,
                    TelephonePassword = flyingClubMemberDoc.TelephonePassword,
                    MediaCode = flyingClubMemberDoc.MediaCode,
                    DowngradeProhibitFlag = flyingClubMemberDoc.DowngradeProhibitFlag,
                    SSAMemberNumber = flyingClubMemberDoc.SSAMemberNumber,
                    OnlineActivationFlag = flyingClubMemberDoc.OnlineActivationFlag,
                    OverrideGroupValidations = flyingClubMemberDoc.OverrideGroupValidations,
                    NameOnCard = flyingClubMemberDoc.NameOnCard,
                    CenturianFlag = flyingClubMemberDoc.CenturianFlag,
                    OverrideValidationFlag = flyingClubMemberDoc.OverrideValidationFlag,
                    OCDBFlag = flyingClubMemberDoc.OCDBFlag,
                    VAInstantGoldCampaignId = flyingClubMemberDoc.VAInstantGoldCampaignId,
                    LastTierChangeDate = flyingClubMemberDoc.LastTierChangeDate,
                    FastTrackId = flyingClubMemberDoc.FastTrackId,
                    InstantSilverCampaignId = flyingClubMemberDoc.InstantSilverCampaignId,
                    VAPartnerFileId = flyingClubMemberDoc.VAPartnerFileId,
                    VAStatusChangeTimeStamp = flyingClubMemberDoc.VAStatusChangeTimeStamp,
                    VRedSharedFlag = flyingClubMemberDoc.VRedSharedFlag,
                    EnrollmentChannel = flyingClubMemberDoc.EnrollmentChannel,
                    MemberShipCardDetails = flyingClubMemberDoc.MemberShipCardDetails,
                    Status = flyingClubMemberDoc.Status,
                    LastUpdatedBy = flyingClubMemberDoc.LastUpdatedBy,
                    MergeToMemberNumber = flyingClubMemberDoc.MergeToMemberNumber
                }
            };
        }
    }
}
