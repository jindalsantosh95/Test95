namespace VAA.EAPI.Microservices.CRM.Notification.Constants
{
    public static class Constants
    {
        public const string NotificationTopicName = "servicenotifications";
        public const string NotificationSubscriberName = "FlightOrderNotif";
        public const string AircraftChangeNotifSubscriberName = "AircraftChangeNotif";       
        public const string NotificationReqEventStoreSubject = "Crm.Notif.FlightOrderNotifRequest";
        public const string CheckinAlert = "Check-in window is open now for your flight ";
        public const string NotifReqType = "deep_link";
        public const string NotifReqContent = "va://acount";
        public const string DeviceTypeAndroid = "android";
        public const string DeviceTypeIos = "ios";
    }
}
