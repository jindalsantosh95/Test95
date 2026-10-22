namespace VAA.EAPI.Microservices.Retail.Order.Data.Constants
{
    public static class Environment
    {
        public const string OrderCreateEventHandlerTopicName = "order";
        public const string OrderCreateSubscriptionName = "OrderCreateSubscriber";
        public static readonly string OrderCollection = "OrderCollection";
        public static readonly string NDCOrderCollection = "NDCOrderCollection";
        public static readonly string DatabaseName = "DatabaseName";
        public static readonly string OrderDBEndpoint = "OrderDBEndpoint";
        public const string OrderReadEventHandlerTopicName = "ticket";
        public const string OrderReadSubscriptionName = "TicketOrderDetailSubscriber";
        public static readonly string RefundReason = "RefundReason";
        public const string OrderStoreSubscriptionName = "OrderStoreEventSubscriber";
        public const string OrderStoreEventHandlerTopicName = "orderrequesteventmessage";
        public const string NDCOrderStoreEventHandlerTopicName = "ndcorderrequesteventmessage";
        public const string NDCOrderStoreSubscriptionName = "NDCOrderStoreEventSubscriber";
        public const string NDCNameUpdateEventHandlerTopicName = "ndcnameupdateeventmessage";
        public const string NDCArcDocumentUpdateEventHandlerTopicName = "ndcarcdocumentupdateeventmessage";
        public const string NDCArcDocumentUpdateSubscriptionName = "NDCArcDocumentUpdateEventSubscriber";
        public const string NDCNameUpdateSubscriptionName = "NDCNameUpdateEventSubscriber";
        public const string UpdateOrderDocumentSubscriptionName = "UpdateOrderDocumentSubscriber";
        public const string OrderServiceBusConnectionString = "OrderServiceBusConnectionString";
        public const string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string OrderDataServiceBusConnectionString = "OrderDataServiceBusConnectionString";
        public static readonly string OrderDataServiceBusTopicName = "OrderDataServiceBusTopicName";
        public const string InsurancePurchaseStatusUpdateTopicName = "insurancepurchasestatuseventmessage";
        public const string InsurancePurchaseStatusSubscriptionName = "insurancepurchasestatuseventsubscriber";
        public const string NDCSeatUpdateSubscriberTopicName = "ndcupdateseateventmessage";
        public const string NDCSeatUpdateSubscriptionName = "NDCSeatUpdateSubscriber";
        public const string CancelOrderSubscriberTopicName = "ndccancelpnreventmessage";
        public const string CancelOrderSubscriptionName = "CancelPnrEventMessageSubscriber";
        public const string NDCTicketUpdateSubscriberTopicName = "ndcupdateticketeventmessage";
        public const string NDCTicketUpdateSubscriptionName = "NDCTicketUpdateSubscriber";
        public const string CustomerOrderAssociation = "CustomerOrderAssociation";
        public const string NDCOrderHistoryUpdateSubscriberTopicName = "ndcupdateorderhistoryeventmessage";
        public const string NDCOrderHistoryUpdateSubscriptionName = "NDCOrderHistoryUpdateSubscriber";

        //Future Bookings
        public static readonly string OrderSCVDBEndpoint = "OrderSCVDBEndpoint";
        public static readonly string OrderDB = "OrderDB";
        public static readonly string OrdersCollection = "OrdersCollection";
        public static readonly string EnableMockView = "EnableMockView";

        //Notifications
        public static readonly string NotificationServiceBusConnectionString = "NotificationServiceBusConnectionString";
        public const string NotificationOrderReadBatchSize = "NotificationOrderReadBatchSize";
        public static readonly string FlightCollection = "FlightCollection";

        //PassengerRetrieval
        public const string PassengerFlightServiceBusConnectionString = "PassengerFlightServiceBusConnectionString";
        public const string FlightArrivalDetailsTopic = "FlightArrivalDetailsTopic";
        public const string FlightArrivalDetailsSubscriber = "FlightArrivalDetailsSubscriber";
        public const string FlightArrivalServiceBusConnectionString = "FlightArrivalServiceBusConnectionString";

    }
}

