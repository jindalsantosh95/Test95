namespace VAA.EAPI.GraphQL.Domain.Loyalty.Constants
{
    public class Application
    {
        public static readonly string SupplementarySilverAuditSubCategoryLinking = "Linking";

        public const string CardLinksPartnerCode = "ACENT";
        public static readonly string ListOfProductsCacheKey = "ReferenceData_PRODUCTDETAILS_CACHE_KEY";
        public static readonly string ListOfPartnersCacheKey = "ReferenceData_PARTNERPROFILE_CACHE_KEY";

        public const int FlightActivityNoOfDays = -190;
        public static readonly string FlightActivityNoOfDaysErrMessage = "Flight Activity Date cannot be greater than 183 days for Retro";
        public static readonly string TxnNotQualifiedForPoints = "No accrual rule could be found. Please send a screenshot of the details you captured in the LOOPS screen to Loyalty Operations.";

        //Future Flights
        public static readonly string FCNumber = "FCNumber";
        public static readonly string PhoneNo = "PhoneNo";
        public static readonly string Email = "Email";
        public const string VirginAccountTierCode = "VAC";

        //autostampfc upgrade vac
        public static readonly string UpgradeSuccessMessage = "Account Upgraded Successful";
        public static readonly string AutostampFailureMessage = "Stamping process failed due to exception";
        public static readonly string AutostampPnrNotAvailable = "No eligible PNRs identified for FC Auto Stamping";
        public static readonly string[] RelatedTxnExcludedProducts = ["Flight", "Flight Bonus"];

        public static readonly string DefaultRedemptionCanxReason = "Outside 24h of travel";
        public static readonly string DefaultAccrualCanxReason = "Other";
        public static readonly string[] VredPartnerType = { "Ground Partner", "Financial Partner" };
        public static readonly string CancelltionSuccessMsg = "Cancellation Request Processed Successfully";
        public static readonly string CancelltionFailureMsg = "Cancellation failed for one or more transactions as follows";
        public static readonly string[] AutoCanxReasons = { "Within 24h of travel", "Outside 24h of travel", "Agent Error (Supervisor only)" };

        public static readonly string LoyaltySyncCustType = "Loyalty";

        public const string PNRNotFoundErrorCode = "1070";

        public static readonly string[] ValidMemberStatuses = { "ACTIVE", "INACTIVE", "FROZEN", "FRAUD", "SUSPENDED", "CANCELLED" };
        public const string PATransactionChannel = "PAUSER";
        public const string PostingTypeCode = "MI";
    }
}
