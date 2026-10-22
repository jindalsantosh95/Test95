using System;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.FlyingClubMember
{
    public class FlyingClubMember
    {
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string CustomerId { get; set; }
        public IEnumerable<MemberAttribute> MemberAttributes { get; set; }
        public string SchemaVersion { get; set; }
        public string Brand { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string AutoAccrualStartDate { get; set; }
        public DateTime? GoldStartDate { get; set; }
        public int? RetainSilvercounter { get; set; }
        public string CorporateMembershipNumber { get; set; }
        public int? RetainGoldCounter { get; set; }
        public string LastAccrualTxnProcessDate { get; set; }
        public string LastRedemptionTxnProcessedDate { get; set; }
        public string LastTransactionProcessedDate { get; set; }
        public string LoungeNumber { get; set; }
        public string MemberNumber { get; set; }
        public string MemberType { get; set; }
        public string GroupId { get; set; }
        public string HeadOfGroupMemberNumber { get; set; }
        public int? TotalTierPoints { get; set; }
        public int? VSOnlyTotalTierPoints { get; set; }
        public int? VSFlownSectors { get; set; }
        public string ProgramId { get; set; }
        public string TierCode { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public string Status { get; set; }
        public string MergeToMemberNumber { get; set; }
        public string TestMemberFlag { get; set; }
        public string TermsAndConditionsFlag { get; set; }
        public string TelephonePassword { get; set; }
        public string MediaCode { get; set; }
        public string DowngradeProhibitFlag { get; set; }
        public string SSAMemberNumber { get; set; }
        public string OnlineActivationFlag { get; set; }
        public string OverrideGroupValidations { get; set; }
        public string NameOnCard { get; set; }
        public string CenturianFlag { get; set; }
        public string OverrideValidationFlag { get; set; }
        public string OCDBFlag { get; set; }
        public string VAInstantGoldCampaignId { get; set; }
        public string LastTierChangeDate { get; set; }
        public string FastTrackId { get; set; }
        public string InstantSilverCampaignId { get; set; }
        public string VAPartnerFileId { get; set; }
        public DateTime? VAStatusChangeTimeStamp { get; set; }
        public string VRedSharedFlag { get; set; }
        public string EnrollmentChannel { get; set; }
        public MembershipCardDetails MemberShipCardDetails { get; set; }
    }
}