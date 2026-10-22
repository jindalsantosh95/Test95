namespace VAA.EAPI.Microservices.Retail.Order.Payment.Constants
{
    public class ErrorCodes
    {
        public static readonly string ServiceBusError = "G034";
        public static readonly string NullOrEmptyOrderReference = "1055";
        public static readonly string InvalidOrNullOrEmptyChannel = "G028";

        public static readonly string InvalidOrNullOrEmptyPaymentInfo = "1070";

        public static readonly string NullOrEmptyTransactionId = "1059";
        public static readonly string InvalidOrNullOrEmptyTransactionDateTime = "G010";
        public static readonly string NullOrEmptyTransactionAmount = "1060";
        public static readonly string InvalidOrNullOrEmptyPaymentMethod = "1061";
        public static readonly string InvalidOrNullOrEmptyPaymentType = "1062";
        public static readonly string InvalidOrNullOrEmptyPaymentSubType = "1067";
        public static readonly string InvalidOrNullOrEmptyCardName = "1068";
        public static readonly string InvalidOrNullOrEmptyCardNumber = "1063";
        public static readonly string InvalidOrNullOrEmptyCardExpiry = "G010";
        public static readonly string InvalidOrNullOrEmptyPaymentStatus = "1069";

        public static readonly string InvalidOrNullOrEmptyPaymentCurrency = "1064";
        public static readonly string InvalidOrNullOrEmptyPaymentAuthCode = "1070";
        public static readonly string InvalidOrNullOrEmptyPSPName = "1065";
        public static readonly string InvalidOrNullOrEmptyPSPId = "1066";
        public static readonly string DeltaTokenAPI = "G044";

        public static readonly string DeltaServicesErrorCode = "G051";
        public static readonly string CosmosErrorCode = "G008";
        public static readonly string CpdErrorCode = "G066";
        public static readonly string DefaultTechErrorCode = "G009";
        public static readonly string HolidayPaymentProcessingError = "H001";
        public static readonly string HolidayPaymentLoginError = "H002";
        public static readonly string HolidayPaymentValidationError = "H003";
    }
}
