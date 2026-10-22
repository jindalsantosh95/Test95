
namespace VAA.EAPI.Microservices.Security.Constants
{
    public class Application
    {
        // Enviromental settings
        public static readonly string ContenTypeApplicationJson = "application/json";
        public const double HttpClientTimeOutInMilliSeconds = 1000;
        public static readonly string HeaderParameterLocation = "location";

        // APIM Subscription Events
        public static readonly string EventSpecversion = "1.0";
        public static readonly string CloudEventTimeFormat = "yyyy-MM-dd hh:mm:ss tt";

        public const string APIMSubscriptionEventTopicName = "apimsubscriptioneventmessages";
        public const string APIMSubscriptionCreateEventSubscriptionName = "CreateAPIMSubscriptionMessageSubscriber";
        public const string APIMSubscriptionDeleteEventSubscriptionName = "DeleteAPIMSubscriptionMessageSubscriber";
        public const string APIMSubscriptionUpdateEventSubscriptionName = "UpdateAPIMSubscriptionMessageSubscriber";

        // APIM Subscription Settings
        public static string MSSubscriptionAPIBaseUrl = "https://management.azure.com";
        public static readonly string CreateAPIMUserUri = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/users/{3}?api-version=2021-08-01";
        public static readonly string APIMSubscriptionUri = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/subscriptions/{3}?api-version=2021-08-01";
        public static readonly string APIMSubscriptionKeysUri = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/subscriptions/{3}/listSecrets?api-version=2021-08-01";
        public static readonly string SubscriptionScope = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/products/{3}";
        public static readonly string APIMSubscriptionConfigFilePath = "Infrastructure\\Configuration\\APIMSubscriptionConfig.json";

    }
}
