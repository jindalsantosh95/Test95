namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Constants
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
        public static readonly string MissingCustomerDetailsErrorCode = "1072";
        public static readonly string MissingOrderAmendmentsErrorCode = "1073";
        public static readonly string MissingFlightItemsErrorCode = "1074";
        public static readonly string MissingCustomerReferenceErrorCode = "1075";
        public static readonly string MissingOrderRefId = "10126";
        public static readonly string InvalidChannelErrorCode = "G028";
        public static readonly string InvalidGender = "10133";
        public static readonly string InvalidDOB = "G020";
        public static readonly string InValidSegmentError = "F001";
        public static readonly string MissingAir4PNR = "F002";
        public static readonly string FlightNoopError = "F003";
        public static readonly string SSROrderExistsForCustomer = "F004";
        public static readonly string IAmCancelError = "F005";
        public static readonly string PartialFlowIsPastIndicator = "F006";

        public static readonly string InvalidDOBCode = "W001";
        public static readonly string InvalidGenderCode = "W002";
        public static readonly string SabreManageMultipleDocError = "W003";

        #endregion

    }
}
