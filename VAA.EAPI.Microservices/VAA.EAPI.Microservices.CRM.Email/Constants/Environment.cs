namespace VAA.EAPI.Microservices.CRM.Email.Constants
{
    public class Environment
    {
        public static readonly string RefundFailureFromAddress = "RefundFailureFromAddress";
        public static readonly string RefundFailureToAddress = "RefundFailureToAddress";
        public static readonly string RefundFailureCcAddress = "RefundFailureCcAddress";
        public static readonly string RefundFailureSubject = "RefundFailureSubject";
        public static readonly string RefundFailureTemplate = "RefundFailureTemplate";
        public static readonly string RFCRefundFailureTemplate = "RFCRefundFailureTemplate";
        //Adobe Campaign Maanager
        public static readonly string ACMSessionToken = "ACMSessionToken";
        public static readonly string ACMCommunicationChannel = "ACMCommunicationChannel"; // 0 = Email and 1 = SMS
        public static readonly string RetryCounter = "RetryCounter";
        public static readonly string DelayTime = "DelayTime";
        public static readonly string EnableEmailMSAuthentication = "EnableEmailMSAuthentication";
        public static readonly string InviteEmailLink = "InviteEmailLink";
        public static readonly string InviteEmailTemplate = "InviteEmailTemplate";
        public static readonly string Servicebus = "InviteEmailTemplate";
        public static readonly string BookingAcknowledgmentTemplate = "BookingAcknowledgmentTemplate";
        public static readonly string BookingCancellationTemplate = "BookingCancellationTemplate";
        public static readonly string HoldBookingTemplate = "HoldBookingTemplate";
        public static readonly string ServiceBusConnectionString = "ServiceBusConnectionString";
        //VA Account Email Trigger
        public static readonly string B2CPasswordResetLink = "B2CPasswordResetLink";
        public static readonly string VACEnrolEmailTemplate = "VACEnrolEmailTemplate";

        public static readonly string PNRCommonV6EndPoint = "PNRCommonV6EndPoint";
        public static readonly string TicketingProcessV3EndPoint = "TicketingProcessV3EndPoint";
    }
}
