namespace VAA.EAPI.Orchestration.Notifications.Constants
{
    public static class Constants
    {
        public const string NotificationTopicName = "servicenotifications";
        public const string AircraftSwapTopicName = "asm";

        public const string CheckinOpenSubscriptionName = "CheckinOpen";
        public const string SeatSelectionReminderSubscriptionName = "SeatSelectionReminder";
        public const string StarlinkEnabledFlightSubscriptionName = "StarlinkEnabledFlight";
        public const string FlightOrderDataSubscriptionName = "FlightOrderData";
        public const string CheckinCustProfileDataSubscriptionName = "CheckinCustProfileData";
        public const string AircraftSwapsSubscriptionName = "ASM_Swap";
        public const string AcSwapsFlightDataRequestSubscriptionName = "AcSwapsFlightDataRequest";

        public const string FlightDetailEventSubject = "FlightOperations.FlightMs.FlightDetail";
        public const string SeatSelectionReminderRequestEventSubject = "Orches.Notif.SeatSelectionReminderRequest";
        public const string StarlinkEnabledFlightEventSubject = "Orches.Notif.StarlinkEnabledFlight";
        public const string FlightOrderDataReqEventSubject = "Retail.Order.data.FlightOrderDataRequest";
        public const string CheckinFlightCustProfileReqEventSubject = "Crm.CP.CheckinFlightOrderCustProfileRequest";        
        public const string FlightOrderNotifReqEventSubject = "Crm.Notif.FlightOrderNotifRequest";
        public const string AircraftChangeNotifReqEventSubject = "Crm.Notif.AircraftChangeNotifRequest";
        public const string AcSwapsFlightDataRequest = "Orches.Notif.AcSwapsFlightDataRequest";
        public const string ApimSubscriptionKey = "Ocp-Apim-Subscription-Key";
        public const string NotificationTypeCheckInOpen = "CheckInOpen";
        public const string NotificationTypeSeatSelectionReminder = "SeatSelectionReminder";
        public const string NotificationTypeAircraftSwap = "AircraftSwap";
        public const string NotificationTypeStarlink = "Starlink";
        public const string DefaultStarlinkCustomEventName = "emit_starlink";
        public const string DefaultStarlinkCampaign = "Starlink";
        public const string DefaultStarlinkWifiCampaign = "WiFi";


        public const string IropNotificationTopicName = "iropservicenotifications";
        public const string IropSubscriptionName = "IropOrchestratorSubscriber";       
        public const string IropNotificationReceivedTelemetryEvent = "IROP_NOTIFICATION_RECEIVED";
   
    }
}
