namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Constants
{
    public class Environment
    {
        public static readonly string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";
        public static readonly string MicroServiceDBKey = "MicroServiceDBKey";
        public static readonly string MicroServiceEventStoreTableName = "MicroServiceEventStoreTableName";
        public static readonly string MicroServiceResponseTableName = "MicroServiceResponseTableName";
        public static readonly string OrderPaymentServiceBusTopicName = "OrderPaymentServiceBusTopicName";
        public static readonly string RefundWindowInHours = "RefundWindowInHours";
        public static readonly string PnrCommonV6EndPoint = "PnrCommonV6EndPoint";
        public static readonly string TicketingProcessV3EndPoint = "TicketingProcessV3EndPoint";
        public static readonly string CreditCardV2EndPoint = "CreditCardV2EndPoint";
        public static readonly string RemarksV3EndPoint = "RemarksV3EndPoint";
        public static readonly string TicketCosmosDBEndPoint = "TicketCosmosDBEndPoint";
        public static readonly string TicketCosmosDBName = "TicketCosmosDBName";
        public static readonly string TicketCosmosDBCollection = "TicketCosmosDBCollection";
        public static readonly string TimeToLiveForTicketDocumentAfterCappEvent = "TtlForTicketDocumentAfterCappEvent";
    }
}
