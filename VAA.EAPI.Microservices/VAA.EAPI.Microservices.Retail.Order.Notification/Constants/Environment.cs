namespace VAA.EAPI.Microservices.Retail.Order.Notification.Constants
{
    public static class Environment
    {
        public static readonly string MockResponseEnabled = "MockResponseEnabled";
        public static readonly string HeaaderParameterlocation = "location";
        public static readonly string OrderNotificationResponseUrl = "OrderNotificationResponseUrl";
        public static readonly string MicroServiceResponseTableName = "MicroServiceResponseTableName";
        public static readonly string MicroServiceDBKey = "MicroServiceDBKey";
        public const string ResponsePollingCount = "ResponsePollingCount";
        public const string ResponseDelayTimeInMS = "ResponseDelayTimeInMS";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";
        public static readonly string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string OpenApiDescription = "OpenApi__Description";
        public static readonly string OpenApiTokenUrl = "OpenApi__TokenUrl";
        public static readonly string OpenApiScope = "OpenApi__Scope";
        public static readonly string HeaderRetryAfter = "Retry-After";
        public static readonly string HeaderPaymentStatusRetryAfterValue = "OrderNotifyStatusRetryAfterValue";

    }
}
