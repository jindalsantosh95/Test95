namespace VAA.EAPI.Microservices.Retail.Holidays.Constants
{
    public static class Application
    {
        public static readonly string CancelledBookingErrorCode = "HMS001";
        public static readonly string QuoteBookingErrorCode = "HMS002";
        public static readonly string BookingStatusMissingErrorCode = "HMS003";
        public static readonly string OptionStatusMissingErrorCode = "HMS004";
        public static readonly string CruiseBookingErrorCode = "HMS005";
        public static readonly string ExpiredBookingErrorCode = "HMS006";
        public static readonly string InvalidRequestParamsErrorCode = "HMS007";
        public static readonly string TboxErrorCode = "HOL002";
        public static readonly string HolidaysApiRequestValidationErrorCode = "HOL001";
        public static readonly string HolidaysApiLeadPaxValidationErrorCode = "HOL003";
        public static readonly string CruiseDivision = "VHCMP";
        public static readonly string HolidayMSGenericErrorCode = "HOL000";

        public static readonly string BankTransferPaymentMethod = "BTRF";
        public static readonly string CreditCardPaymentMethod = "CRCD";
        public static readonly string DisneyElementGroupCode = "DISNEY";
    }
}
