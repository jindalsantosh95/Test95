namespace VAA.EAPI.Microservices.Loyalty.Sky.Constants
{
    public class ErrorCodes
    {
        public static readonly string MiscellaneousTechnicalError = "G009";
        public static readonly string CustomerIneligbleorIncorrectTicketInfo = "10180";
        public static readonly string SkyRetroFeedbackFailure = "10188";

        //Sky eligibility and feedback errors
        public static readonly string InvalidFlightDate = "S101";
        public static readonly string InvalidOperatingFlightNumber = "S102";
        public static readonly string FlightNumberandDetailsMismatch = "S103";
        public static readonly string PassengerNotFound = "S104";
        public static readonly string SkyTechnicalIssue = "S105";
        public static readonly string InvalidTicketType = "S106";
        public static readonly string PassengerNotFoundDuringUpdate = "S107";
        public static readonly string ClaimRequestAlreadyPending = "S201Y";
        public static readonly string FFPTaggedDifferent = "S201R";
        public static readonly string TechnicalIssueSystemDown = "S301";
        public static readonly string RetroClaimUnavailable = "S302";
        public static readonly string InvalidOperatingAirline = "S303";
        public static readonly string TechnicalIssueHub = "S304";
        public static readonly string AdditionalInfoNeeded = "S666";
        public static readonly string FlightNumberInvalid = "S888";
        public static readonly string TechnicalIssueDuringUpdate = "S997";
    }
}
