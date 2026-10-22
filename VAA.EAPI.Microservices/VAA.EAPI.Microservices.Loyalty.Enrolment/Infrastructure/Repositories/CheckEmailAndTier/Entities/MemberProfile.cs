using Newtonsoft.Json;
using System;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CheckEmailAndTier.Entities
{
    public class MemberProfile
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
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

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

        public string PartitionKey { get; set; }

        public string ProgramId { get; set; }

        public int RetainGoldCounter { get; set; }

        public int RetainSilvercounter { get; set; }

        public string SourceIdentifier { get; set; }

        public string SourceSystem { get; set; }

        public string Status { get; set; }

        public string TelephonePassword { get; set; }

        public string TermsAndConditionsFlag { get; set; }

        public string TestMemberFlag { get; set; }

        public string TierCode { get; set; }

        public int TotalTierPoints { get; set; }

        public string VaInstantGoldCampaignId { get; set; }

        public long VsFlownSectors { get; set; }

        public int VsOnlyTotalTierPoints { get; set; }

        public MembershipAttribute[] MemberAttributes { get; set; }

        public string SchemaVersion { get; set; }

        public string GroupID { get; set; }

        public string HeadOfGroupMemberNumber { get; set; }

        public string SSAMemberNumber { get; set; }

        public string OCDBFlag { get; set; }

        public string VAPartnerFileId { get; set; }

        public string VAStatusChangeTimeStamp { get; set; }

    }
}
