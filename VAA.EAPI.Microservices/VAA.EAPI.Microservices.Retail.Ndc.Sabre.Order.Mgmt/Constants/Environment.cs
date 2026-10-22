namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Constants
{
    public class Environment
    {
        public static readonly string OrderChangeSeatAddServiceBusTopicName = "OrderChangeSeatAddServiceBusTopicName";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";
        public static readonly string OrderChangeResponseUrl = "OrderChangeResponseUrl";
        public const string ResponseDelayTimeInMS = "ResponseDelayTimeInMS";
        public const string ResponseDelayForOrderChangeTimeInMS = "ResponseDelayForOrderChangeTimeInMS";
        public const string ResponsePollingCount = "ResponsePollingCount";
        public const string RetryCount = "RetryCount";
        public const string MaxRetriesCount = "MaxRetriesCount";
        public static string ServiceBusConnectionString = "ServiceBusConnectionString";
        public const string DeletionRetryCounterLimit = "DeletionRetryCounterLimit";
        public static readonly string OrderDataServiceBusConnectionString = "OrderDataServiceBusConnectionString";
        public static readonly string OrderDataServiceBusTopicName = "OrderDataServiceBusTopicName";
        public static readonly string SabreEndpoint = "SabreEndpoint";
        public static readonly string ConversationId = "ConversationId";
        public static readonly string FromPartyId = "FromPartyId";
        public static readonly string ToPartyId = "ToPartyId";
        public static readonly string CreateSessionEndPoint = "CreateSessionEndPoint";
        public static readonly string GenderList = "GenderList";
        public static readonly string ManageCustomerResponseUrl = "ManageCustomerResponseUrl";
        public static readonly string SplitPnrResponseUrl = "SplitPnrResponseUrl";

        public static readonly string OrderCancelResponseUrl = "OrderCancelResponseUrl";
        public static readonly string DirectTicketingPCC = "DirectTicketingPCC";
        public static readonly string InterLineOperatingSegments = "InterLineOperatingSegments";

    }
}
