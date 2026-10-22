using System;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class MemberProfileDTO : CosmosDocument
    {
        public string AutoAccrualStartDate { get; set; }

        public string Brand { get; set; }

        public string CenturianFlag { get; set; }

        public string CorporateMembershipNumber { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedOn { get; set; }

        public string CustomerId { get; set; }

        public string DocType { get; set; }

        public string DowngradeProhibitFlag { get; set; }

        public string EnrollmentDate { get; set; }

        public string FastTrackId { get; set; }

        public string GoldStartDate { get; set; }

        public string InstantSilverCampaignId { get; set; }

        public string LastAccrualTxnProcessDate { get; set; }

        public string LastRedemptionTxnProcessedDate { get; set; }

        public string LastTierChangeDate { get; set; }

        public string LastTransactionProcessedDate { get; set; }

        public string LastUpdatedBy { get; set; }

        public string LastUpdatedOn { get; set; }

        public string LoungeNumber { get; set; }

        public string MediaCode { get; set; }

        public string MemberNumber { get; set; }

        public MembershipCard MemberShipCardDetails { get; set; }

        public string MemberType { get; set; }

        public string MergeToMemberNumber { get; set; }

        public string NameOnCard { get; set; }

        public string OnlineActivationFlag { get; set; }

        public string OverrideGroupValidations { get; set; }

        public string OverrideValidationFlag { get; set; }

        public string ProgramId { get; set; }

        public int? RetainGoldCounter { get; set; }

        public int? RetainSilvercounter { get; set; }

        public string SourceIdentifier { get; set; }

        public string SourceSystem { get; set; }

        public string Status { get; set; }

        public string TelephonePassword { get; set; }

        public string TermsAndConditionsFlag { get; set; }

        public string TestMemberFlag { get; set; }

        public string TierCode { get; set; }

        public int? TotalTierPoints { get; set; }

        public string VaInstantGoldCampaignId { get; set; }

        public long? VsFlownSectors { get; set; }

        public int? VsOnlyTotalTierPoints { get; set; }

        public MembershipAttribute[] MemberAttributes { get; set; }

        public string GroupID { get; set; }

        public string HeadOfGroupMemberNumber { get; set; }

        public string SSAMemberNumber { get; set; }

        public string OCDBFlag { get; set; }

        public string VAPartnerFileId { get; set; }

        public string VAStatusChangeTimeStamp { get; set; }

        public string SchemaVersion { get; set; } = Constants.Application.SchemaVersion;

        public MemberProfileDTO()
        {
            //required for seriaization
        }

        public MemberProfileDTO(MemberProfile memberProfile)
        {
            if (memberProfile == null) throw new ArgumentNullException(nameof(memberProfile));

            Id = memberProfile.Id;

            PartitionKey = memberProfile.PartitionKey;

            AutoAccrualStartDate = memberProfile.AutoAccrualStartDate ?? string.Empty;

            Brand = memberProfile.Brand ?? string.Empty;

            CenturianFlag = memberProfile.Brand ?? string.Empty;

            CorporateMembershipNumber = memberProfile.CorporateMembershipNumber ?? string.Empty;

            CreatedBy = memberProfile.CreatedBy ?? string.Empty;

            CreatedOn = memberProfile.CreatedOn ?? string.Empty;

            CustomerId = memberProfile.CustomerId ?? string.Empty;

            DocType = memberProfile.DocType ?? string.Empty;

            DowngradeProhibitFlag = memberProfile.DowngradeProhibitFlag ?? string.Empty;

            EnrollmentDate = memberProfile.EnrollmentDate ?? string.Empty;

            FastTrackId = memberProfile.FastTrackId ?? string.Empty;

            GoldStartDate = memberProfile.GoldStartDate ?? string.Empty;

            InstantSilverCampaignId = memberProfile.InstantSilverCampaignId ?? string.Empty;

            LastAccrualTxnProcessDate = memberProfile.LastAccrualTxnProcessDate ?? string.Empty;

            LastRedemptionTxnProcessedDate = memberProfile.LastRedemptionTxnProcessedDate ?? string.Empty;

            LastTierChangeDate = memberProfile.LastTierChangeDate ?? string.Empty;

            LastTransactionProcessedDate = memberProfile.LastTransactionProcessedDate ?? string.Empty;

            LastUpdatedBy = memberProfile.LastUpdatedBy ?? string.Empty;

            LastUpdatedOn = memberProfile.LastUpdatedOn ?? string.Empty;

            LoungeNumber = memberProfile.LoungeNumber ?? string.Empty;

            MediaCode = memberProfile.MediaCode ?? string.Empty;

            MemberNumber = memberProfile.MemberNumber ?? string.Empty;

            MemberShipCardDetails = memberProfile.MemberShipCardDetails;

            MemberType = memberProfile.MemberType ?? string.Empty;

            MergeToMemberNumber = memberProfile.MergeToMemberNumber ?? string.Empty;

            NameOnCard = memberProfile.NameOnCard ?? string.Empty;

            OnlineActivationFlag = memberProfile.OnlineActivationFlag ?? string.Empty;

            OverrideGroupValidations = memberProfile.OverrideGroupValidations ?? string.Empty;

            OverrideValidationFlag = memberProfile.OverrideValidationFlag ?? string.Empty;

            ProgramId = memberProfile.ProgramId ?? string.Empty;

            RetainGoldCounter = memberProfile.RetainGoldCounter;

            RetainSilvercounter = memberProfile.RetainSilvercounter;

            SourceIdentifier = memberProfile.SourceIdentifier ?? string.Empty;

            SourceSystem = memberProfile.SourceSystem ?? string.Empty;

            Status = memberProfile.Status ?? string.Empty;

            TelephonePassword = memberProfile.TelephonePassword ?? string.Empty;

            TermsAndConditionsFlag = memberProfile.TermsAndConditionsFlag ?? string.Empty;

            TestMemberFlag = memberProfile.TestMemberFlag ?? string.Empty;

            TierCode = memberProfile.TierCode ?? string.Empty;

            VaInstantGoldCampaignId = memberProfile.VaInstantGoldCampaignId ?? string.Empty;

            VsFlownSectors = memberProfile.VsFlownSectors;

            MemberAttributes = memberProfile.MemberAttributes;

            GroupID = memberProfile.GroupID ?? string.Empty;

            HeadOfGroupMemberNumber = memberProfile.HeadOfGroupMemberNumber;

            SSAMemberNumber = memberProfile.SSAMemberNumber ?? string.Empty;

            OCDBFlag = memberProfile.OCDBFlag ?? string.Empty;

            VAPartnerFileId = memberProfile.VAPartnerFileId ?? string.Empty;

            VAStatusChangeTimeStamp = memberProfile.VAStatusChangeTimeStamp ?? string.Empty;

            TotalTierPoints = memberProfile.TotalTierPoints;

            VsOnlyTotalTierPoints = memberProfile.VsOnlyTotalTierPoints;
        }


    }
}
