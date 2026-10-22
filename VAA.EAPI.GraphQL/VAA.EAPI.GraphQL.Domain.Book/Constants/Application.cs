namespace VAA.EAPI.GraphQL.Domain.Book.Constants
{
    public class Application
    {
        public static readonly string Flight = "flight";
        public static readonly string AdultPtc = "ADULT";
        public static readonly string ChildPtc = "CHILD";
        public static readonly string InfantPtc = "INFANT";
        public static readonly string TeenPtc = "TEEN";
        public static readonly string ResourceType = "CONFIRMATION";
        public static readonly string StringSpace = " ";
        public static readonly string DeltaPtcAdult = "ADT";
        public static readonly string DeltaPtcChild = "CNN";
        public static readonly string DeltaPtcInfant = "INF";
        public static readonly string DeltaPtcTeen = "GBE";
        public const string Basket = "Basket";
        public const string AirportDetails = "AirportDetails";
        public const string AirlineDetails = "AirlineDetails";
        public const string CabinDetails = "CabinDetails";
        public static readonly string InvalidStatus = "INVALID";
        public static readonly string ValidStatus = "VALID";
        public static readonly string PaymentTypeHeader = "formOfPayment";
        public const string DeltaErrorCode = "G051";
        public const string FraudCheckDeltaErrorCode = "ROR4011";
        public const string FraudCheckErrorCode = "2001";
        public const string FraudCheckErrorMessage = "FRAUD_CHECK";
        public const string APMVoidErrorCode = "2002";
        public const string APMCaptureErrorCode = "2003";
        public const string DeltaGenericErrorCode = "2007";
        public static readonly string ErrorFilePath = "ErrorCodes\\ErrorCodes.json";
        public static readonly string VSFQTProgramme = "VS";
        public static readonly string DefaultCurrencyCode = "USD";
        public static readonly string DeltaGenderCodeMale = "M";
        public static readonly string DeltaGenderCodeFemale = "F";
        public static readonly string DeltaGenderCodeUndisclosed = "U";
        public static readonly string DeltaGenderCodeUnspecified = "X";
        public static readonly string SeatMSChannelId = "NBF";
        public static readonly string DeltaMainCabin = "MAIN";
        public static readonly string DeltaComfortCabin = "DCP";
        public static readonly string SeatMapStatusOccupied = "OCCUPIED";
        public static readonly string SeatMapStatusFree = "FREE";
        public static readonly string SeatMapStatusSelected = "SELECTED";
        public static readonly string SeatMapStatusBlocked = "BLOCKED";
        public static readonly string ChannelId = "ChannelId";
        public static readonly string BasketId = "BasketId";
        public static readonly string OfferNoteligibleForPWPErrorCode = "2013";
        public static readonly string PointsToCurrencyConverterNotAvailableErrorCode = "2012";
        public static readonly string PointsNotSufficientForPWPErrorCode = "2011";
        public static readonly string USCountryName = "United States Of America";
        public static readonly string USCountryCode = "US";
        public static readonly string VACTier = "VAC";
        public const string VoucherRedemptionFeatureFlag = "EnableVoucherRedemption";

        //Allianz Constants
        public static readonly string UserAgent = "user-agent";
        public static readonly string AnzTripIndicatorOneWay = "ONE_WAY";
        public static readonly string AnzTripIndicatorRoundTrip = "ROUND_TRIP";
        public static readonly string AnzRequestOrigin = "Standard";
        public static readonly string AnzRequestOriginPage = "Passenger";
        public static readonly string AnzPaymentMethod = "Cash";
        public static readonly string AnzRequestCategory = "Airfare";
        public static readonly string AnzLanguage = "en";
        public static readonly int AnzOfferVersionId = 4;
        public static readonly string XForwardedFor = "X-Forwarded-For";

        public static readonly string NoQuoteAvailableErrorMessage = "No Quote Available";
        public static readonly string NoQuoteAvailableErrorCode = "RQ01";

        //VAC EnrolmentConstants
        public static readonly string VACEnrolmentMemberType = "Individual";
        public static readonly string VACEnrolmentCustomerType = "VirginAccount";
        public static readonly string VACEnrolmentStatus = "Active";
        public static readonly string VACEnrolmentMediaCode = "WEB";
        public static readonly string VACEnrolmentChannel = "WEB";

        //Payment MS
        public const string EngagementScoreURL = "/engagement";
        public const string SynchronyDataShareTokenURL = "/synchronytoken";
        public const string PaymentOrderReferenceIdMSUrl = "/paymentReferenceId";
        public const string PaymentCaptureMSUrl = "/capture";
        public const string PaymentVoidMSUrl = "/void";
        public const string PaymentDetailsMSUrl = "/paymentDetails/{paymentReferenceId}";
        public const string ApplicationStatusPath = "/applicationStatus/{token}";
    }
}
