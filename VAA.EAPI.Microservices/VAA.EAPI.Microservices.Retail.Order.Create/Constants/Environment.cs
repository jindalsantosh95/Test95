namespace VAA.EAPI.Microservices.Retail.Order.Create.Constants
{
    public class Environment
    {
        public static readonly string BookingProcessV3EndPoint = "BookingProcessV3EndPoint";
        public static readonly string PnrCommonV6EndPoint = "PnrCommonV6EndPoint";
        public static readonly string TicketingProcessV3EndPoint = "TicketingProcessV3EndPoint";
        public static readonly string OrderCreateResponseUrl = "OrderCreateResponseUrl";
        public static readonly string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string OrderCreateEventType = "OrderCreateEventType";
        public static readonly string OrderCreateEventSubject = "OrderCreateEventSubject";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";
        public const string ResponsePollingCount = "ResponsePollingCount";
        public const string ResponseDelayTimeInMS = "ResponseDelayTimeInMS";
    }
}
