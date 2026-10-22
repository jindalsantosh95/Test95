using System.Security.Cryptography.X509Certificates;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Constants
{
    public class Application
    {
        public static readonly string SpecVersion = "1";
        public static readonly string EventSubject = "OCDB Sync Failure";
        public static readonly string ContentType = "application/json";
        public static readonly string DataVersion = "1.0";
        public static readonly string ApplicationVersion = "1 0";
        public static readonly string DebugOption = "N";
        public static readonly string LrecModificationIndicator = "C";
        public static readonly string DateFormat = "yyyyMMdd";
        public static readonly string VirginTest = "VirginTest";
        public static readonly string EventGridSource = "EnrolmentAPI";
        public static readonly string EventType = "com.virginatlantic.cloudevents.CustomerLoyaltyEvent";
        public static X509Certificate2 DeltaCertificate;
        public const string ManagaeAccountUpdateTopicName = "manageaccount";
        public const string UpdateOCDBEventSubscriptionName = "UpdateOCDBSyncMessageSubscriber";
        public static readonly string ApplicationId = "DC";
        // Command Response 
        public static readonly string HeaderParameterlocation = "location";
        public static readonly string CommandResponseStatusKey = "LoyaltySync";
        public static readonly string RequestSuccessfullyInitiatedMessage = "Request successfully initiated";
        public const string CloudEventTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public static readonly string EligibilityIndicator = "D";
    }
}