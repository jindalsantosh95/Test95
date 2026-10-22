using Newtonsoft.Json;
namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class FlyingClubMember
    {
        [JsonProperty("AutoAccrualStartDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? AutoAccrualStartDate { get; set; }

        [JsonProperty("Brand")]
        public string Brand { get; set; }

        [JsonProperty("CenturianFlag", NullValueHandling = NullValueHandling.Ignore)]
        public string CenturianFlag { get; set; }

        [JsonProperty("CorporateMembershipNumber")]
        public string CorporateMembershipNumber { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("DBLastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DbLastUpdatedOn { get; set; }

        [JsonProperty("DBLastUpdatedSrc")]
        public string DbLastUpdatedSrc { get; set; }

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("DowngradeProhibitFlag", NullValueHandling = NullValueHandling.Ignore)]
        public string DowngradeProhibitFlag { get; set; }

        [JsonProperty("DowngradeProhibitReason")]
        public string DowngradeProhibitReason { get; set; }

        [JsonProperty("EnrollmentChannel")]
        public string EnrollmentChannel { get; set; }

        [JsonProperty("EnrollmentDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? EnrollmentDate { get; set; }

        [JsonProperty("FastTrackId")]
        public string FastTrackId { get; set; }

        [JsonProperty("GoldStartDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? GoldStartDate { get; set; }

        [JsonProperty("HeadOfGroupMemberNumber")]
        public string HeadOfGroupMemberNumber { get; set; }

        [JsonProperty("HouseHoldId")]
        public string HouseHoldId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("InstantSilverCampaignId")]
        public string InstantSilverCampaignId { get; set; }

        [JsonProperty("LastAccrualTxnProcessDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastAccrualTxnProcessDate { get; set; }

        [JsonProperty("LastRedemptionTxnProcessedDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastRedemptionTxnProcessedDate { get; set; }

        [JsonProperty("LastTierChangeDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastTierChangeDate { get; set; }

        [JsonProperty("LastTransactionProcessedDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastTransactionProcessedDate { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdatedOn { get; set; }

        [JsonProperty("LoungeNumber")]
        public string LoungeNumber { get; set; }

        [JsonProperty("MediaCode")]
        public string MediaCode { get; set; }

        [JsonProperty("MemberNumber")]
        public string MemberNumber { get; set; }

        [JsonProperty("MemberShipCardDetails")]
        public MembershipCard MemberShipCardDetails { get; set; }

        [JsonProperty("MemberType")]
        public string MemberType { get; set; }

        [JsonProperty("MemberUID")]
        public string MemberUid { get; set; }

        [JsonProperty("MergeToMemberNumber")]
        public string MergeToMemberNumber { get; set; }

        [JsonProperty("NameOnCard")]
        public string NameOnCard { get; set; }

        [JsonProperty("OldUserName")]
        public string OldUserName { get; set; }

        [JsonProperty("OnlineActivationFlag", NullValueHandling = NullValueHandling.Ignore)]
        public string OnlineActivationFlag { get; set; }

        [JsonProperty("OverrideHouseholdValidations", NullValueHandling = NullValueHandling.Ignore)]
        public string OverrideHouseholdValidations { get; set; }

        [JsonProperty("OverrideValidationFlag", NullValueHandling = NullValueHandling.Ignore)]
        public string OverrideValidationFlag { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("ProgramId")]
        public string ProgramId { get; set; }

        [JsonProperty("RetainGoldCounter", NullValueHandling = NullValueHandling.Ignore)]
        public string? RetainGoldCounter { get; set; }

        [JsonProperty("RetainSilvercounter", NullValueHandling = NullValueHandling.Ignore)]
        public string? RetainSilvercounter { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("TelephonePassword")]
        public string TelephonePassword { get; set; }

        [JsonProperty("TermsAndConditionsFlag", NullValueHandling = NullValueHandling.Ignore)]
        public string TermsAndConditionsFlag { get; set; }

        [JsonProperty("TestMemberFlag", NullValueHandling = NullValueHandling.Ignore)]
        public string TestMemberFlag { get; set; }

        [JsonProperty("TierCode")]
        public string TierCode { get; set; }

        [JsonProperty("TotalTierPoints", NullValueHandling = NullValueHandling.Ignore)]
        public string TotalTierPoints { get; set; }

        [JsonProperty("UKCCMemberNumber")]
        public string UkccMemberNumber { get; set; }

        [JsonProperty("UnsuccessfulQAndACounter", NullValueHandling = NullValueHandling.Ignore)]
        public string UnsuccessfulQAndACounter { get; set; }

        [JsonProperty("VAAStatusChangeTimeStamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? VaaStatusChangeTimeStamp { get; set; }

        [JsonProperty("VAInstantGoldCampaignId")]
        public string VaInstantGoldCampaignId { get; set; }

        [JsonProperty("VRedSharedFlag", NullValueHandling = NullValueHandling.Ignore)]
        public string VRedSharedFlag { get; set; }

        [JsonProperty("VSFlownSectors", NullValueHandling = NullValueHandling.Ignore)]
        public string VsFlownSectors { get; set; }

        [JsonProperty("VSOnlyTotalTierPoints", NullValueHandling = NullValueHandling.Ignore)]
        public string VsOnlyTotalTierPoints { get; set; }

        [JsonProperty("SchemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonProperty("Active")]
        public string Active { get; set; }

        [JsonProperty("CardExpiryDate")]
        public string CardExpiryDate { get; set; }

        [JsonProperty("CardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty("CardReplacementReason")]
        public string CardReplacementReason { get; set; }

        [JsonProperty("GroupId")]
        public string GroupId { get; set; }

        [JsonProperty("SSAMemberNumber")]
        public string SSAMemberNumber { get; set; }


        [JsonProperty("MemberAttributes")]
        public List<MemberAttributes> MemberAttributes { get; set; }

        [JsonProperty("_etag")]
        public string _etag { get; set; }

    }

}
