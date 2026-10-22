namespace VAA.EAPI.Microservices.CRM.Notification.Constants
{
    public static class Environment
    {
        public const string NotificationServiceBusConnectionString = "NotificationServiceBusConnectionString";
        public const string AirshipApiUri = "AirshipApiUri";
        public const string AirshipToken = "AirshipToken";
        public const string AirshipClientId = "AirshipClientId";
        public const string AirshipClientSecret = "AirshipClientSecret";
        public const string AirshipAuthority = "AirshipAuthority";
        public const string AirshipApplicationId = "AirshipApplicationId";

        public const string FlightCancelledAlertMessage = "FlightCancelledAlertMessage";
        public const string FlightCancelledContent = "FlightCancelledContent";
        public const string CheckInOpenedAlertMessage = "CheckInOpenedAlertMessage";
        public const string CheckInOpenedContent = "CheckInOpenedContent";
        public const string FlightTimeChangedAlertMessage = "FlightTimeChangedAlertMessage";
        public const string FlightTimeChangedContent = "FlightTimeChangedContent";
        public const string DefaultAlertMessage = "DefaultAlertMessage";
        public const string DefaultContent = "DefaultContent";

        public const string RetryCounter = "RetryCounter";
        public const string DelayTime = "DelayTime";
        public const string AircraftChangeTemplate = "AircraftChangeTemplate";
        public const string ACMSessionToken = "ACMSessionToken";
        public const string ACMCommunicationChannel = "ACMCommunicationChannel"; // 0 = Email and 1 = SMS
        public const string EnableEmailMSAuthentication = "EnableEmailMSAuthentication";
        public const string TestEmail = "TestEmail";


    }
}
