using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class LoyaltyMember
    {
        public enum MemberTypeEnum
        {
            Individual = 1,
            Household = 2,
            Account = 3
        }

        public MemberTypeEnum? MemberType { get; set; }

        public enum ProgramNameEnum
        {
            [EnumMember(Value = "Virgin Atlantic Flying Club")]
            VirginAtlanticFlyingClub = 1
        }

        public ProgramNameEnum? ProgramName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum StatusEnum
        {
            /// <summary>
            /// Enum Active for value: Active
            /// </summary>
            [EnumMember(Value = "Active")]
            Active = 1,
            /// <summary>
            /// Enum Expired for value: Expired
            /// </summary>
            [EnumMember(Value = "Expired")]
            Expired = 2,
            /// <summary>
            /// Enum Frozen for value: Frozen
            /// </summary>
            [EnumMember(Value = "Frozen")]
            Frozen = 3,
            /// <summary>
            /// Enum Inactive for value: Inactive
            /// </summary>
            [EnumMember(Value = "Inactive")]
            Inactive = 4,
            /// <summary>
            /// Enum Merged for value: Merged
            /// </summary>
            [EnumMember(Value = "Merged")]
            Merged = 5,
            /// <summary>
            /// Enum Deceased for value: Deceased
            /// </summary>
            [EnumMember(Value = "Deceased")]
            Deceased = 6,
            /// <summary>
            /// Enum Suspended for value: Suspended
            /// </summary>
            [EnumMember(Value = "Suspended")]
            Suspended = 7,
            /// <summary>
            /// Enum Cancelled for value: Cancelled
            /// </summary>
            [EnumMember(Value = "Cancelled")]
            Cancelled = 8,
            /// <summary>
            /// Enum ActivePending for value: Active Pending
            /// </summary>
            [EnumMember(Value = "Active Pending")]
            ActivePending = 9,
            /// <summary>
            /// Enum Fraud for value: Fraud
            /// </summary>
            [EnumMember(Value = "Fraud")]
            Fraud = 10,
            /// <summary>
            /// Enum PendingCancellation for value: Pending Cancellation
            /// </summary>
            [EnumMember(Value = "Pending Cancellation")]
            PendingCancellation = 11,
            /// <summary>
            /// Enum RouteDeal for value: Route Deal
            /// </summary>
            [EnumMember(Value = "Route Deal")]
            RouteDeal = 12
        }
        /// <summary>
        /// Status of Member
        /// </summary>
        /// <value>Status of Member</value>
        [DataMember(Name = "status", EmitDefaultValue = false)]

        public StatusEnum? Status { get; set; }

        public LoyaltyMember(string id = default, MemberTypeEnum? memberType = default, ProgramNameEnum? programName = default, string memberNumber = default, string authorisedAccountUser = default, string referredBySSA = default, bool? aliasFoundFlag = false, bool? vaaAmexIndicator = false, bool? householdMemberFlag = false, int? recordCount = default, int? noOfConsecYrinGold = default, string flyingClubCardNumber = default, StatusEnum? status = default, DateTime? startDate = default, DateTime? statusChangeTimeStamp = default, string headOfHouseholdMemberNumber = default, DateTime? lastTransactionDate = default, string cardName = default, string currentTier = default, DateTime? currentTierStartDate = default, DateTime? currentTierEndDate = default, int? goldCounter = default, int? silverCounter = default, int? totalTierPoints = default, string campaignCode = default, DateTime? goldStartDateOverride = default, bool? testMember = false, string previousTier = default, DateTime? tierChangeDate = default, string mediaCode = default, string enrollmentChannel = default, int? retainPointsTotal = default, int? rollingPointsTotal = default, string partnerUserId = default, string householdPartnerUserId = default, string mergedMasterPartnerUserId = default, int? pointsRequiredForUpgrade = default, int? pointsRequiredForRetain = default, string onlineActivation = default, string centurionFlag = default, bool? termsAndConditionsAccepted = false, List<Award> awards = default, Guardian guardianDetails = default, PartnerMember partnerMember = default)
        {
            Id = id;
            MemberType = memberType;
            ProgramName = programName;
            MemberNumber = memberNumber;
            AuthorisedAccountUser = authorisedAccountUser;
            ReferredBySSA = referredBySSA;
            // use default value if no "aliasFoundFlag" provided
            if (aliasFoundFlag == null)
            {
                AliasFoundFlag = false;
            }
            else
            {
                AliasFoundFlag = aliasFoundFlag;
            }
            // use default value if no "vaaAmexIndicator" provided
            if (vaaAmexIndicator == null)
            {
                VaaAmexIndicator = false;
            }
            else
            {
                VaaAmexIndicator = vaaAmexIndicator;
            }
            // use default value if no "householdMemberFlag" provided
            if (householdMemberFlag == null)
            {
                HouseholdMemberFlag = false;
            }
            else
            {
                HouseholdMemberFlag = householdMemberFlag;
            }
            RecordCount = recordCount;
            NoOfConsecYrinGold = noOfConsecYrinGold;
            FlyingClubCardNumber = flyingClubCardNumber;
            Status = status;
            StartDate = startDate;
            StatusChangeTimeStamp = statusChangeTimeStamp;
            HeadOfHouseholdMemberNumber = headOfHouseholdMemberNumber;
            LastTransactionDate = lastTransactionDate;
            CardName = cardName;
            CurrentTier = currentTier;
            CurrentTierStartDate = currentTierStartDate;
            CurrentTierEndDate = currentTierEndDate;
            GoldCounter = goldCounter;
            SilverCounter = silverCounter;
            TotalTierPoints = totalTierPoints;
            CampaignCode = campaignCode;
            GoldStartDateOverride = goldStartDateOverride;
            // use default value if no "testMember" provided
            if (testMember == null)
            {
                TestMember = false;
            }
            else
            {
                TestMember = testMember;
            }
            PreviousTier = previousTier;
            TierChangeDate = tierChangeDate;
            MediaCode = mediaCode;
            EnrollmentChannel = enrollmentChannel;
            RetainPointsTotal = retainPointsTotal;
            RollingPointsTotal = rollingPointsTotal;
            PartnerUserId = partnerUserId;
            HouseholdPartnerUserId = householdPartnerUserId;
            MergedMasterPartnerUserId = mergedMasterPartnerUserId;
            PointsRequiredForUpgrade = pointsRequiredForUpgrade;
            PointsRequiredForRetain = pointsRequiredForRetain;
            OnlineActivation = onlineActivation;
            CenturionFlag = centurionFlag;
            // use default value if no "termsAndConditionsAccepted" provided
            if (termsAndConditionsAccepted == null)
            {
                TermsAndConditionsAccepted = false;
            }
            else
            {
                TermsAndConditionsAccepted = termsAndConditionsAccepted;
            }
            Awards = awards;
            GuardianDetails = guardianDetails;
            PartnerMember = partnerMember;
        }

        public string Id { get; set; }
        public string MemberNumber { get; set; }
        public string AuthorisedAccountUser { get; set; }
        public string ReferredBySSA { get; set; }
        public bool? AliasFoundFlag { get; set; }
        public bool? VaaAmexIndicator { get; set; }
        public bool? HouseholdMemberFlag { get; set; }
        public int? RecordCount { get; set; }
        public int? NoOfConsecYrinGold { get; set; }
        public string FlyingClubCardNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StatusChangeTimeStamp { get; set; }
        public string HeadOfHouseholdMemberNumber { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public string CardName { get; set; }
        public string CurrentTier { get; set; }
        public DateTime? CurrentTierStartDate { get; set; }
        public DateTime? CurrentTierEndDate { get; set; }
        public int? GoldCounter { get; set; }
        public int? SilverCounter { get; set; }
        public int? TotalTierPoints { get; set; }
        public string CampaignCode { get; set; }
        public DateTime? GoldStartDateOverride { get; set; }
        public bool? TestMember { get; set; }
        public string PreviousTier { get; set; }
        public DateTime? TierChangeDate { get; set; }
        public string MediaCode { get; set; }
        public string EnrollmentChannel { get; set; }
        public int? RetainPointsTotal { get; set; }
        public int? RollingPointsTotal { get; set; }
        public string PartnerUserId { get; set; }
        public string HouseholdPartnerUserId { get; set; }
        public string MergedMasterPartnerUserId { get; set; }
        public int? PointsRequiredForUpgrade { get; set; }
        public int? PointsRequiredForRetain { get; set; }
        public string OnlineActivation { get; set; }
        public string CenturionFlag { get; set; }
        public bool? TermsAndConditionsAccepted { get; set; }
        public List<Award> Awards { get; set; }
        public Guardian GuardianDetails { get; set; }
        public PartnerMember PartnerMember { get; set; }
    }
}
