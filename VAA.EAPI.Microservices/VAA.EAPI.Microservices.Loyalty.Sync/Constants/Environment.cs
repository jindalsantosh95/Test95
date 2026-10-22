namespace VAA.EAPI.Microservices.Loyalty.Sync.Constants
{
    public class Environment
    {
        public static readonly string Source = "EventSourceUrl";
        public static readonly string AppChannelName = "AppChannelName";
        public static readonly string ApplicationId = "ApplicationId";
        public static readonly string TestLab = "TestLab";
        public static readonly string EventGridUrl = "EventGridUrl";
        public static readonly string EventGridTopicKey = "EventGridTopicKey";
        public static readonly string KeyVaultUri = "KeyVaultUri";
        public static readonly string DeltaCertificateValue = "DeltaCertificateValue";
        public static readonly string DeltaLoyaltyProfileSyncEndPoint = "DeltaLoyaltyProfileSyncEndPoint";
        public const string LoyaltyEventHandlerTopicName = "enrolcustomer";
        public const string LoyaltyEventSubscriptionName = "EnrolCustomerLoyaltySyncSubscriber";
        public const string PSDSEventSubscriptionName = "EnrolCustomerPSDSSyncSubscriber";
        public const string Air4PassengerV4EndPoint = "Air4PassengerV4EndPoint";
        public const string EnablePSDSSync = "EnablePSDSSync";
        public const string EnrolmentChannelList = "EnrolmentChannelList";
        public const string UpdateOCDBEventHandlerTopicName = "updateenroledcustomerdata";
        public const string UpdateOCDBEventSubscriptionName = "UpdateOCDBSyncMessageSubscriber";
        public static readonly string DFAppChannelName = "DFAppChannelName";
        public static readonly string DeltaCurrencyConvertServiceEndpoint = "DlCurrencyConvertServiceEndpoint";
        // Command Status 
        public static readonly string CommandStatusLocation = "CommandStatusLocation";
        public static readonly string CommandStatusPollingDelayMS = "CommandStatusPollingDelayMS";
        public static readonly string CommandStatusPollingRetryCount = "CommandStatusPollingRetryCount";

        public static readonly string SFPDSyncName = "SyncSFPD";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";
        public static readonly string EventTypeCustomerSync = "EventTopicTypeCustomerSync";
        public static readonly string EventTypePSDSSync = "EventTopicTypPSDSSync";
        public static readonly string ServiceBusConnectionString = "ServiceBusConnectionString";

    }
}
