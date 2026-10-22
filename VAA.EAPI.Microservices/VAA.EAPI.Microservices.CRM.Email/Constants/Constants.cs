namespace VAA.EAPI.Microservices.CRM.Email.Constants
{
    public class Constants
    {
        public static readonly string Ccpchannel = "CCPChannel";
        public static readonly string Ccptemplate = "CCPTemplate";
        public static readonly string CcpfromAddress = "CCPFromAddress";
        public const string EmailEventHandlerTopicName = "enrolcustomer";
        public const string EmailEventSubscriptionName = "EmailSubscriber";
        public const string WelcomeEmailEventSubscriptionName = "WelcomeEmailSubscriber";
        public const string OTPEmailEventSubscriptionName = "OTPEmailSubscriber";
        public const string ManageAccountTopicName = "manageaccount";
        public const string ManageAccountEmailSubscriptionName = "ManageAccountEmailSubscriber";
        public const string HouseHoldInviteEmailSubscriptionName = "HouseHoldInviteEmailSubscriber";
        public const string HouseholdAccountTopicName = "householdaccount";
        public const string VACEnrolEmailSubscriptionName = "VACEnrolEmailSubscriber";
        public const string SeatConfirmEmailSubscriptionName = "SeatConfirmationEmailSubscriber";
        public const string SeatconfirmationEmailMessageTopicName = "seatconfirmationemailmessagetopicname";
        public const string StoreAppointmentTopicName = "storeappointmentbooking";
        public const string StoreAppointmentEmailSubscriptionName = "StoreAppointmentEmailSubscriber";
    }
}