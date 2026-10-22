namespace VAA.EAPI.GraphQL.Domain.Account.Constants
{
    public class Application
    {
        public static readonly string VirginMoney = "Virgin Money";
        public static readonly string Email = "EmailAddress";
        public static readonly string FcNumber = "FlyingClubNumber";
        public const string VouchersError = "G052";
        public const string ActivityError = "G053";
        public const string MemberCustomAttributesError = "G067";
        public const string MemberDetailsError = "G054";
        public const string FlightsError = "G055";
        public const string PointsDotComError = "G057";
        public const string MembershipLevelForRed = "FF";
        public const string MembershipLevelForSilver = "FO";
        public const string MembershipLevelForGold = "GM";
        public const string DefaultLanguage = "en";
        public const string UsCountryCode = "us";
        public const string UsCurrencyCode = "USD";
        public const string DefaultCurrencyCode = "GBP";
        public const string EmailUpdateError = "G058";
        public const string MemberAddressError = "G060";
        public const string MemberUpdatePhoneNumberError = "G059";
        public const string MemberUpdatePreferenceError = "G061";
        public const string LinkedMemberRetrievalError = "G063";
        public const string ParentAccountInactiveErrorCode = "10007";
        public const string Under18StandardEnrolmentErrorCode = "1027";
        public const string Under18StandardEnrolmentErrorMessage = "A parent or legal guardian must create their account through the parent’s Flying Club profile.";
        public static readonly string HouseHoldAccount = "Household";
        public static readonly string LinkedAccount = "Linked";
        public static readonly string IndividualAccount = "Individual";
        public static readonly string DefaultHHAccountName = "Your HouseHold Account";
        public static readonly string ProcessedStatus = "Allocated";
        public const string TicketStatusUpdateSuccessMessage = "Ticket Number Updated successfully";
        public static readonly string VirginAtlantic = "Virgin Atlantic";
        public static string loginValidationError = "10159";
        public const string GenerateWalletPassError = "G065";
        public const int leaderFcNumberCount = 1;
        public const string PendingInviteListMemberErrorCode = "10170";
        public const string TotalHouseHoldAccountErroCode = "10169";
        public const string InviteLinkedMemberErrorCode = "10165";
        public const string MissingPointsErrorCode = "10179";
        public const string NameValidationErrorCode = "10175";
        public const string VredCreateTransactionErrorCode = "10179V";
        public const string DateValidateErrorCode = "10174";
        public const string VSPointsClaimError = "G066";
        public const string VredMarketingFeatureFlag = "VredMarketingFeatureFlag";
        public const string BannerRetrievalErrorCode = "10196";
        public const bool TripsDetailsMockResponse = true;
        public static readonly string ReferenceCountriesEndpoint = "countries";
        public const string FlyingClubApplicationTokenEndpoint = "/application-token";
        public const string FlyingClubApplicationTokenValidationEndpoint = "/application-token/validate";
        public const string FlyingClubApplicationTokenRole = "Loyalty.Identity.ApplicationToken.Issue";
        public const string AccountLinkingProofEndpoint = "/account-linking-proof";
        public const string AccountLinkingProofRedemptionEndpoint = "/account-linking-proof/redeem";
        public const string AccountLinkingProofRedeemRole = "Loyalty.Identity.AccountLinkingProof.Redeem";
        public const string FlyingClubAccountSummaryRole = "Loyalty.AccountSummary.Read";
        public static readonly string LinkedMemberChild = "Linked Member-Child";
    }
}
