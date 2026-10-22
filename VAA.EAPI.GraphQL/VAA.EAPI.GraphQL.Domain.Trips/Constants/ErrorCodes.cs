namespace VAA.EAPI.GraphQL.Domain.Trips.Constants
{
    public class ErrorCodes
    {
        public static readonly string RetrieveTripDetailsErrorCode = "MA02";
        public static readonly string DefaultErrorCode = "MA01";
        public static readonly string TechnicalError = "G009";
        public static readonly string SelectSpecialAssistanceErrorCode = "MA03";
        public static readonly string SelectSpecialMealErrorCode = "MA04";
        public static readonly string SelectSeatErrorCode = "MA05";
        public static readonly string RetrieveLoyaltyProgrammesErrorCode = "MA06";
        public static readonly string ManageLoyaltyProgrammeErrorCode = "MA07";
        public static readonly string InvalidCarrierCodeErrorCode = "MA08";
        public static readonly string InvalidSeatUpgradeConfirmation = "MA09";
        public static readonly string RetrieveTripRefreshErrorCode = "MA12";
        public static readonly string DeltaErrorCode = "ROR4090";
        public static readonly string RetrieveHolidayBooking = "MA13";
        public static readonly string RetrieveHolidayBookingValidation = "MA14";
        public static readonly string HolidayPaymentErrorCode = "MA15";
        public static readonly string HolidaysApiRequestValidationErrorCode = "HOL001";
    }
}
