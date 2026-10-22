namespace VAA.EAPI.Web.Common.Constants
{
    public class Environment
    {
        // Azure Active Directory
        public static readonly string AADAuthority = "AAD:Authority";
        public static readonly string AADTenant = "AAD:TenantId";
        public static readonly string AADV1Issuer = "AAD:TokenV1Issuer";

        // My Identity
        public static readonly string AppId = "MyIdentity:AppID";
        public static readonly string ClientSecret = "MyIdentity:ClientSecret";

        // Open API
        public static readonly string OpenApiDescription = "OpenApi__Description";
        public static readonly string OpenApiTokenUrl = "OpenApi__TokenUrl";
        public static readonly string OpenApiScope = "OpenApi__Scope";
        public static readonly string CommonCachingSlidingExpirationInDay = "CommonCachingSlidingExpirationInDay";
        public static readonly string CommonCachingAbsoluteExpirationInDay = "CommonCachingAbsoluteExpirationInDay";
        public static readonly string TimeToWaitBeforeNextRecall = "TimeToWaitBeforeNextRecall";

        // Logging
        public static readonly string ReleaseEnvironment = "environment";
        public static readonly string LoggingProvider = "LoggingProvider";
        public static readonly string DatadogLogsEndpoint = "DatadogLogsEndpoint";
        public static readonly string DatadogApiKey = "DatadogApiKey";
        public static readonly string DatadogService = "DatadogService";
        public static readonly string DatadogSource = "DatadogSource";
        public static readonly string DatadogVersion = "DatadogVersion";
        public static readonly string DatadogTags = "DatadogTags";
        public static readonly string DatadogBaggageAllowList = "DatadogBaggageAllowList";
        public static readonly string DatadogMinimumLogLevel = "DatadogMinimumLogLevel";
        public static readonly string DatadogQueueCapacity = "DatadogQueueCapacity";
        public static readonly string DatadogBatchSize = "DatadogBatchSize";
        public static readonly string DatadogFlushIntervalMilliseconds = "DatadogFlushIntervalMilliseconds";

        // Postback URL
        public static readonly string PostbackUrl = "PostbackUrl";
        // GraphQL
        public static readonly string GraphQLUrl = "GraphQL:Url";
        public static readonly string GraphQLShowMockResponse = "GraphQL:ShowMockResponse";
        public static readonly string GraphQLOfferSearchPqId = "GraphQL:OfferSearch:PqId";
        public static readonly string RetrieveBasketPqId = "GraphQL:RetrieveBasketPqId";

        // Caching
        public static readonly string HttpClientTimeOutInMilliSeconds = "HttpClientTimeOutInMilliSeconds";
        /// <summary>
        /// To enable microservice caching define {HttpMethod}:{path/query} with a timespan value, this will be used for cache expiry.
        ///     e.g.
        ///             "Cache:post:https://tempuri.org/api/token": "expiry=00:00:10",
        ///             ...
        /// </summary>
        public static readonly string RedisCacheConnectionString = "RedisCacheConnectionString";

        public static readonly string MicroServiceClientId = "MicroServiceClientId";
        public static readonly string MicroServiceClientSecret = "MicroServiceClientSecret";
        public static readonly string BasketUri = "BasketUri";
        public static readonly string OfferSubscriptionAutomationKey = "OfferSubscriptionAutomationKey";
        public static readonly string BasketSubscriptionAutomationKey = "BasketSubscriptionAutomationKey";
        public static readonly string OrderConfirmationMockResponse = "OrderConfirmationMockResponse";

        public static string BasketLink = "BasketLink";
        public static string ContactLink = "ContactLink";
        public static string TermsLink = "TermsLink";
        public static string PassengerLink = "PassengerLink";
        public static string BookCreateOrderPqId = "BookCreateOrderPqId";
    }
}
