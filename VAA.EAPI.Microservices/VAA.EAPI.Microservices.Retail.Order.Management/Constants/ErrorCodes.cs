namespace VAA.EAPI.Microservices.Retail.Order.Management.Constants
{
    public class ErrorCodes
    {

        #region OrderChange

        public static readonly string InvalidAttributeLenght = "1028";
        public static readonly string SchDepTimeFormat = "1029";
        public static readonly string SchArrTimeFormat = "1030";
        public static readonly string InvalidPtcLength = "1043";
        public static readonly string FirstNameMissing = "G014";
        public static readonly string LastNameMissing = "G015";
        public static readonly string InvalidOriginDestId = "1045";
        public static readonly string InvalidOriginAirportCode = "1046";
        public static readonly string InvalidDestinationAirportCode = "1047";
        public static readonly string InvalidProductType = "1048";
        public static readonly string ActionContextMissing = "1049";
        public static readonly string InvalidSegmentId = "1050";
        public static readonly string InvalidMarketingCarrier = "1051";
        public static readonly string InvalidMarketingFlightNumber = "1052";
        public static readonly string ClassOfServiceMissing = "1053";
        public static readonly string CustomerIDMissing = "1054";
        public static readonly string InvalidEmailFromat = "19";
        public static readonly string DeltaErrorCode = "G026";
        public static readonly string MarriedSegmentErrorCode = "1071";
        public static readonly string MissingCustomerDetailsErrorCode = "1072";
        public static readonly string MissingOrderAmendmentsErrorCode = "1073";
        public static readonly string MissingFlightItemsErrorCode = "1074";
        public static readonly string MissingCustomerReferenceErrorCode = "1075";
        public static readonly string InvalidChannelErrorCode = "G028";
        public static readonly string MissingOrderRefId = "10126";
        public static readonly string InvalidOrderRefId = "10127";
        public static readonly string InvalidGender = "10133";
        public static readonly string InvalidDOB = "G020";
        public static readonly string InValidPTC = "10134";
        public static readonly string MiscellaneousTechnicalError = "G009";
        public static readonly string MandatoryInputFieldsMissing = "15001";

        #region Delta SOA Services Error codes
        public const string DeltaSetupContextErrorCode = "G029";
        public const string DeltaCancelRebookErrorCode = "G030";
        public const string DeltaTearDownErrorCode = "G031";
        public const string DeltaRePriceErrorCode = "G032";
        public const string DeltaTicketingProcessV3ErrorCode = "G033";
        public const string RepriceRollbackErrorCode = "1078";
        public const string UnevenExchangeRollbackErrorCode = "1077";
        public static readonly string MarriedSegmentErrorMessage = "Request cannot be processed due to invalid combination";
        public static readonly string PassengerV4ManageCustomerError = "G045";
        public static readonly string InvalidDOBCode = "W001";
        public static readonly string InvalidGenderCode = "W002";
        public static readonly string DeltaManageMultipleDocError = "W003";
        #endregion

        #endregion

    }
}
