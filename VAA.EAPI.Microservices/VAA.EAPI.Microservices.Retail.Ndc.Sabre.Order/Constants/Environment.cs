namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Constants
{
    public class Environment
    {
        public static readonly string OrderCreateResponseUrl = "OrderCreateResponseUrl";
        public static readonly string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";
        public static readonly string OrderDataServiceBusConnectionString = "OrderDataServiceBusConnectionString";
        public static readonly string OrderDataServiceBusTopicName = "OrderDataServiceBusTopicName";
        public const string ResponsePollingCount = "ResponsePollingCount";
        public const string ResponseDelayTimeInMS = "ResponseDelayTimeInMS";
        public const string DeletionRetryCounterLimit = "DeletionRetryCounterLimit";
        public static readonly string SabreEndpoint = "SabreEndpoint";
        public static readonly string ConversationId = "ConversationId";
        public static readonly string FromPartyId = "FromPartyId";
        public static readonly string ToPartyId = "ToPartyId";
        public const string SSRDelayInMS = "SSRDelayInMS";
        public const string PollingIntervalMs = "PollingIntervalMs";
        public const string MaxAttempts = "MaxAttempts";

        public static readonly string TtlExpiryTopicName = "TtlExpiryTopicName";
    }
}
