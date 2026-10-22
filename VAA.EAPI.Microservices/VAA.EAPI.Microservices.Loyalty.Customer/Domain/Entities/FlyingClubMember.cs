using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities
{
    public class FlyingClubMember
    {
        public string AutoAccrualStartDate { get; set; }
        public string Brand { get; set; }
        public string CenturianFlag { get; set; }
        public string CorporateMembershipNumber { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CustomerId { get; set; }
        public string DocType { get; set; }
        public string DowngradeProhibitFlag { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public string FastTrackId { get; set; }
        public DateTime? GoldStartDate { get; set; }
        public string Id { get; set; }
        public string InstantSilverCampaignId { get; set; }
        public DateTime? LastAccrualTxnProcessDate { get; set; }
        public string LastRedemptionTxnProcessedDate { get; set; }
        public string LastTierChangeDate { get; set; }
        public string LastTransactionProcessedDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LoungeNumber { get; set; }
        public string MediaCode { get; set; }
        public string MemberNumber { get; set; }
        public MemberShipCardDetails MemberShipCardDetails { get; set; }
        public string MemberType { get; set; }
        public string MergeToMemberNumber { get; set; }
        public string NameOnCard { get; set; }
        public string OnlineActivationFlag { get; set; }
        public string OverrideGroupValidations { get; set; }
        public string OverrideValidationFlag { get; set; }
        public string PartitionKey { get; set; }
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
        public int? VsFlownSectors { get; set; }
        public int? VsOnlyTotalTierPoints { get; set; }
        public List<MemberAttribute> MemberAttributes { get; set; }
        public string SchemaVersion { get; set; }
        public string GroupID { get; set; }
        public string HeadOfGroupMemberNumber { get; set; }
        public string SSAMemberNumber { get; set; }
        public string OCDBFlag { get; set; }
        public string VAPartnerFileId { get; set; }
        public string VAStatusChangeTimeStamp { get; set; }

    }
}

