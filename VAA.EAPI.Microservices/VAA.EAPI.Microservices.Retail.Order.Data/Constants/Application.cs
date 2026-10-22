using System;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Constants
{
    public static class Application
    {
        public static readonly string OrderNotificationEventSubject = "";
        public static readonly string EventUtcTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public static readonly string EventSpecversion = "1.0";
        public static readonly string CreateOrderEventSubject = "CreateOrder";
        public static readonly string SchemaVersion = "1";
        public static readonly string ISODateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";
        public static readonly string CreatedBy = "EAPI-OD-MS";
        public static readonly string OrderEvent = "Create";
        public static readonly string OrderCreateResponseEventSubject = "OrderCreateResponseEvent";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";
        public static readonly string ServiceBusTopicNameTicket = "ServiceBusTopicNameTicket";
        public static readonly string OrderCreateEventType = "OrderCreateResponseEventType";
        public static readonly string OrderDBSqlQuery = "SELECT * FROM c  WHERE c.PartitionKey = @partitionKey AND c.DocType = @docType";
        public static readonly string OrderDocType = "Order";
        public static readonly string PnrSearchDocType = "PnrSearch";
        public static readonly string OrderPaymentStatusUpdateSubject = "OrderPaymentStatusUpdate";
        public static readonly DateTime UnixEpochTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static readonly int Random4DigitNum = new Random().Next(1000, 9999);
        public static readonly string Query = "SELECT * from c where c.PartitionKey=@PartitionKey and c.DocType=@DocType";
        public static readonly string OrderReadResponseEventSubject = "OrderReadResponseEvent";
        public static readonly string NDCStoreQuery = "SELECT * from c where c.PartitionKey=@PartitionKey and c.id = @Id";
        public static readonly string NDCStoreQueryWithPnr = "SELECT * from c where c.BookingReference.RLOC = @PNR";
        public static readonly string ScvOrderByPnrQuery = "SELECT * FROM c WHERE c.DocType = @DocType AND EXISTS (SELECT VALUE bookingReference FROM bookingReference IN c.OrderDetails.BookingReferences WHERE bookingReference.RLOC = @Pnr)";
        public static readonly string ScvOrderIdsByPnrQuery = "SELECT c.OrderID FROM c WHERE c.DocType = @DocType AND STARTSWITH(c.OrderID, @Pnr)";
        public static readonly string ScvMatchingOrderIdsQuery = "SELECT c.OrderID FROM c WHERE c.DocType = @DocType AND c.OrderID = @OrderId AND EXISTS (SELECT VALUE s FROM s IN c.SegmentDetails WHERE STARTSWITH(s.ScheduledDepartureTime.Local_Date_Time, @FlightDate)) AND EXISTS (SELECT VALUE p FROM p IN c.CustomerDetails WHERE STRINGEQUALS(TRIM(p.FirstName), @FirstName, true) AND STRINGEQUALS(TRIM(p.LastName), @LastName, true))";
        public static readonly string ScvOrderByOrderIdQuery = "SELECT * FROM c WHERE c.DocType = @DocType AND c.OrderID = @OrderId";
        public static readonly string UpdateOrderDocumentEventSubject = "UpdateOrderDocument";
        public static readonly string OwnerAirline = "VS";
        public static readonly string DateFormat = "yyyy-MM-dd";
        public static readonly string OpenStatus = "OPEN";
        public static readonly string OrderidPrefix = "VS932";
        public static readonly string OrderidSuffix = "24";
        public static readonly string CancelStatus = "Cancelled";
        public static readonly string ZuluDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

        //Future Bookings
        public static readonly string OrderStoreQuery = "SELECT distinct(c.OrderID) FROM c WHERE c.DocType ='CustomerOrderAssociation'AND c.PartitionKey='CustomerOrderAssociation'";
        public static readonly string GetOrderDocsQuery = "SELECT * from c WHERE ARRAY_CONTAINS(@OrderID,c.OrderID)";
        public static readonly string MsgDataNotFound = "No flights found for the given search criteria";

        public const string NotificationTopicName = "servicenotifications";
        public const string FlightOrderFetcherSubscriberName = "FlightOrderDataRequest";

        public static readonly string GetOrderIdsQuery = @"SELECT * FROM c WHERE c.DocType = 'FlightOrders' AND c.FlightPartitionkey = @PartitionKey AND c.FltDate = @FlightDate AND c.FltNumber = @FlightNumber";
        public static readonly string FlightOrderDataEventSubject = "Orches.Notif.FlightOrderDataResponse";
        public static readonly string FlightOrderDataEventType = "Orches.Notif.FlightOrderDataResponse";
        public static readonly string CheckinFlightOrderDataEventSource = "VAA.EAPI.Microservices.Retail.Order.Data";

        //PassengerRetrieval
        public static readonly string PDMEventMessages = "pdmeventmessages";
        public static readonly string FlownPassengerMessage = "FlownPassengerMessage";

    }
}
