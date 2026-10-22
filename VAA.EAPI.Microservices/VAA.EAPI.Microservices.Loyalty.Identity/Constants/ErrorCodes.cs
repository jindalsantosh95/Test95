namespace VAA.EAPI.Microservices.Loyalty.Identity.Constants
{
    public class ErrorCodes
    {
        public static readonly string B2CBusinessError = "601";
        public static readonly string FirstNameMissing = "G014";
        public static readonly string LastNameMissing = "G015";
        public static readonly string InvalidPNR = "1028";
        public static readonly string ExceptionErrorCode = "G035";
        public static readonly string PNRExpiredErrorCode = "1079";
        public static readonly string PassengerNameMisMatchForPNR = "1080";
        public static readonly string InvalidETicketNumber = "1084";
        public static readonly string PassengerNameMisMatchForETicket = "1085";
        public static readonly string TypeMissingOrInvalid = "1086";
        public static readonly string TicketingServiceErrorCode = "G038";
        public static readonly string InvalidMemberErrorCode = "10135";
        public static readonly string UnverifiedAccount = "10136";
        public static readonly string InvalidPassword = "10138";
        public static readonly string InvalidorMissingEmailID = "G013";
        public static readonly string EmailIdMissing = "G050";
        public static readonly string MissingOTP = "10152";
        public static readonly string InvalidOtp = "10153";
        public static readonly string ExpiredOTP = "10154";
        public static readonly string OTPGenerationFail = "10155";
        public static readonly string UnavailableData = "G007";
        public static readonly string UnavailableDataSource = "G008";
        public static readonly string MaxAttemptReached = "10156";
        public static readonly string OTPValidationMaxAttemptReached = "10158";
        public static readonly string TokenDecryptionErrorCode = "10159";
        public static readonly string TokenInvalidOrExpiredErrorCode = "10160";
        public static readonly string InvalidEmailFormat = "10161d";
        public static readonly string EmailExistsInB2C = "10161a";
        public static readonly string MiscellaneousTechnicalError = "G009";
        public static readonly string BadRequest = "400";
        public static readonly string EnrolmentTokenValidationFail = "10199";

    }
}
