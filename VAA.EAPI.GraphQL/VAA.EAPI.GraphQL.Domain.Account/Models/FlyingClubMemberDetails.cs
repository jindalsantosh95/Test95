using HotChocolate;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class FlyingClubMemberDetails
    {
        [GraphQLDescription("Member which can be of Type: HouseHold, Linked and Individual")]
        public HouseHoldMemberType MemberType { get; set; }

        [GraphQLDescription("Member's First Name")]
        public string FirstName { get; set; }

        [GraphQLDescription("The Member's Last Name")]
        public string LastName { get; set; }

        [GraphQLDescription("Date of Birth")]
        public DateTime DateofBirth { get; set; }

        [GraphQLDescription("member's Gender")]
        public string Gender { get; set; }

        [GraphQLDescription("Member's Email")]
        public string Email { get; set; }

        [GraphQLDescription("Member's Contact Number")]
        public string ContactNumber { get; set; }

        [GraphQLDescription("Member's Country Code")]
        public string CountryCode { get; set; }

        [GraphQLDescription("Member's Address")]
        public IEnumerable<AddressCustMS> Address { get; set; }

        [GraphQLDescription("Member's preference on VAA marketing")]
        public bool IsVAAMarketingPreferred { get; set; }

        [GraphQLDescription("Member's preferred partner frequent flyer program")]
        public string PreferredPartnerFFP { get; set; }

        [GraphQLDescription("Head of the house member number")]
        public string HeadOfGroupMemberNumber { get; set; }

        [GraphQLDescription("Member's Tier Code like Red, Gold, Silver")]
        public string TierCode { get; set; }

        [GraphQLDescription("The Member's Preferred Name")]
        public string PreferredName { get; set; }

        [GraphQLDescription("The Member's FC Membership Number")]
        public long FCMembershipNumber { get; set; }

        [GraphQLDescription("The Member's FC Join Date")]
        public DateTime? FCJoinDate { get; set; }

        [GraphQLDescription("The Member's Date tier points reset")]
        public DateTime DateTierPointsReset { get; set; }

        [GraphQLDescription("The Member's deadline for upgrade")]
        public DateTime UpgradeByDate { get; set; }

        [GraphQLDescription("The Member's Tier points")]
        public int TierPoints { get; set; }

        [GraphQLDescription("The Member's Tier points need to reach next tier")]
        public int TierPointsNeedToReachNextTier { get; set; }

        [GraphQLDescription("Member's Flown Miles")]
        public int FlownMiles { get; set; }

        [GraphQLDescription("Cumulative Virgin Points")]
        public string TotalVirginPoints { get; set; }

        [GraphQLDescription("The Member's CustomerID")]
        public string CustomerID { get; set; }

        [GraphQLDescription("ProfileID associated with Member")]
        public string ProfileID { get; set; }

        [GraphQLDescription("The Member Tier points accumulated towards retention")]
        public int RetainTierPoints { get; set; }

        [GraphQLDescription("The Member Tier points need for retention")]
        public int TierPointsNeedToRetain { get; set; }

        [GraphQLDescription("The Member Tier points need for upgrade")]
        public int RollingTierPoints { get; set; }

        [GraphQLDescription("The Member revised VS rolling tier points for future tier evaluation")]
        public int RevisedRollingTierPoints { get; set; }

        [GraphQLDescription("The Member revised VS retain tier points for future tier evaluation")]
        public int RevisedRetainTierPoints { get; set; }

        [GraphQLDescription("Check Member's status")]
        public MembershipStatus MemberStatus { get; set; }

        [GraphQLDescription("Threshold values for different tier")]
        public IEnumerable<TierThresholdDetails> TierThresholdDetails { get; set; }

        public FlyingClubMemberDetails(CustomerDocument customerResponse, CustomerTransactionDocument customerTransactionResponse, VRedResponse vredResponse, IEnumerable<TierThresholdDetails> thresholdValues, IEnumerable<AddressCustMS> primaryAddress, CustomerProfileDTO customerProfile, FcMemberDetails customerTransactionsFlyingClub)
        {
            var flyingClubMember = customerTransactionsFlyingClub.FlyingClubMemberDocuments.FirstOrDefault();
            var preferredPartnerPreference = customerProfile.CustomerProfile.Preferences.FirstOrDefault(x => x.PreferenceType == Common.Constants.Application.PreferredPartnerFFP);
            var preferredPartnerValue = preferredPartnerPreference?.PreferenceValue;

            FirstName = customerResponse?.FirstName;
            LastName = customerResponse?.LastName;
            DateofBirth = customerResponse.DateOfBirth;
            Gender = customerResponse?.Gender;
            Email = customerResponse?.ContactDetails.FirstOrDefault(x => x.ContactType == "Email" && x.Primary == "Y")?.ContactValue;
            ContactNumber = customerResponse?.ContactDetails.FirstOrDefault(x => x.ContactType == "Phone" && x.Primary == "Y")?.ContactValue;
            CountryCode = customerResponse?.ContactDetails.FirstOrDefault(x => x.ContactType == "Phone" && x.Primary == "Y")?.CountryCode;
            Address = primaryAddress;
            IsVAAMarketingPreferred = customerProfile.CustomerProfile.Preferences.FirstOrDefault(x => x.PreferenceType == Common.Constants.Application.PrefenceType)?.PreferenceValue == "Y";
            PreferredPartnerFFP = string.IsNullOrEmpty(preferredPartnerValue) ? null : preferredPartnerValue;
            MemberType = (flyingClubMember?.MemberType.Is("household") ?? false) ? HouseHoldMemberType.Household : ((flyingClubMember?.MemberType.Is("individual") ?? false) && !string.IsNullOrEmpty(flyingClubMember?.HeadOfGroupMemberNumber)) ? HouseHoldMemberType.Linked : HouseHoldMemberType.Individual;
            HeadOfGroupMemberNumber = flyingClubMember?.HeadOfGroupMemberNumber;
            FCMembershipNumber = long.Parse(customerResponse.LoyaltyMemberNumber);
            FCJoinDate = customerTransactionsFlyingClub?.FlyingClubMemberDocuments?.FirstOrDefault()?.EnrollmentDate;
            PreferredName = customerResponse.FirstName;
            TierCode = customerTransactionResponse.TierCode;
            TierPointsNeedToReachNextTier = customerTransactionResponse.TierPointsNeedToReachNextTier;
            TierPointsNeedToRetain = customerTransactionResponse.TierPointsNeedToRetain;
            TierPoints = string.IsNullOrEmpty(customerTransactionResponse.TierPoints.ToString()) ? 0 : customerTransactionResponse.TierPoints;
            DateTierPointsReset = customerTransactionResponse.DateTierPointsReset;
            UpgradeByDate = customerTransactionResponse.UpgradeByDate;
            FlownMiles = customerTransactionResponse.FlownMiles;
            TotalVirginPoints = vredResponse.Current;
            CustomerID = customerResponse.CustomerId;
            RetainTierPoints = customerTransactionResponse.RetainTierPoints;
            RollingTierPoints = customerTransactionResponse.RollingTierPoints;
            MemberStatus = customerTransactionResponse.MemberStatus;
            TierThresholdDetails = thresholdValues;
        }

        public FlyingClubMemberDetails(CustomerDocument customerResponse, CustomerProfileDTO customerProfile, FcMemberDetails customerTransactionsFlyingClub)
        {
            var preferredPartnerPreference = customerProfile.CustomerProfile.Preferences.FirstOrDefault(x => x.PreferenceType == Common.Constants.Application.PreferredPartnerFFP);
            var preferredPartnerValue = preferredPartnerPreference?.PreferenceValue;

            FirstName = customerResponse?.FirstName;
            LastName = customerResponse?.LastName;
            DateofBirth = customerResponse.DateOfBirth;
            Email = customerResponse?.ContactDetails.FirstOrDefault(x => x.ContactType == "Email" && x.Primary == "Y")?.ContactValue;
            ContactNumber = customerResponse?.ContactDetails.FirstOrDefault(x => x.ContactType == "Phone" && x.Primary == "Y")?.ContactValue;
            CountryCode = customerResponse?.ContactDetails.FirstOrDefault(x => x.ContactType == "Phone" && x.Primary == "Y")?.CountryCode;
            IsVAAMarketingPreferred = customerProfile.CustomerProfile.Preferences.FirstOrDefault(x => x.PreferenceType == Common.Constants.Application.PrefenceType)?.PreferenceValue == "Y";
            PreferredPartnerFFP = string.IsNullOrEmpty(preferredPartnerValue) ? null : preferredPartnerValue;
            MemberType = HouseHoldMemberType.Individual;
            TierCode = customerTransactionsFlyingClub?.FlyingClubMemberDocuments?.FirstOrDefault()?.TierCode;
            CustomerID = customerResponse.CustomerId;
        }

        public void ApplyRevisedVsTierPoints(int rollingCoreTierPoints, int retainCoreTierPoints)
        {
            RevisedRollingTierPoints = rollingCoreTierPoints;
            RevisedRetainTierPoints = retainCoreTierPoints;
        }
    }

}
