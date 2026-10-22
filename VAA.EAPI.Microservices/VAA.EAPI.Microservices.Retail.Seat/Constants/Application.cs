namespace VAA.EAPI.Microservices.Retail.Seat.Constants
{
    public static class Application
    {
        public static readonly string IsoDateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        public static readonly string NdcChannel = "NDC";
        public static readonly string ChannelId = "ChannelId";
        public static readonly string CacheKey = "ChannelCachekey";
        public static readonly string ConfigTableName = "ChannelConfig";
        public static readonly string BusinessFault = "BusinessFault";
        public static readonly string SystemFault = "SystemFault";
        public static readonly string ErrorMessage = "ErrorMessage";
        public static readonly string ErrorCode = "ErrorCode";
        public static readonly string SeatDocType = "SeatDoc";
        public static readonly string EventSpecversion = "1.0";
        public static readonly string SeatFulfilmentEventSubject = "SeatFulfilment";
        public static readonly string HeaaderParameterlocation = "location";
        public const string SeatFulFillmentTopicName = "seatfulfilmenteventmessage";
        public const string SeatFulfilmentEventSubscriptionName = "seatfulfilment";
        public static readonly string VendorFopType = "VendorFopType";
        public static readonly string DebugInfo = "DebugInfo";
        public static readonly string DutyCode = "GS";
        public static readonly string AppChannelName = "AppChannelName";
        public static readonly string ApplicationVersion = "1.0";
        public static readonly string TurnOffResponseInfo = "false";
        public static readonly string Air4ServiceApplicationID;
        public static readonly string Air4ServiceEnvironmentName;
        public static readonly string PoolType;
        public static readonly string SqlQuerySeatOfferSearch = "SELECT * FROM c where c.id = @Id";
        public static readonly string ZuluDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
        public static readonly string OrderChangeEventSubject = "OrderChange";
        public const string OrderChangeSeatAddServiceBusTopicName = "orderchangeseatfulfilmenteventmessage";
        public const string OrderSeatFulfilmentEventSubscriptionName = "orderchangeseatfulfilment";
        public const string OrderChangeSeatDeleteServiceBusTopicName = "orderchangeseatdeletioneventmessage";
        public const string OrderSeatDeletionEventSubscriptionName = "orderchangeseatdeletion";
        public static readonly string NbfChannel = "NBF";
        public static readonly string UpdateSeatCommand = "updateSeat";
        public static readonly string FlightSegmentMapPath = "/FlightSegmentSeatMap";
        public static readonly string StandardSeat = "STANDARD";
        public static readonly string PrefferedSeat = "PREFFERED";
        public static readonly string ExitSeat = "EXIT";
        public static readonly string NDCUpdateSeatTopicName = "ndcupdateseateventmessage";
        public static readonly string SeatDeletionEventSubject = "SeatDeletion";
        public const string SeatDeletionServiceBusTopicName = "deleteseatseventmessage";
        public const string SeatDeletionEventSubscriptionName = "deleteseats";
        public static readonly string VendorFopTypeDetailText = "PMNTENG";
        public static readonly string SeatPaymentReceiptEventSubject = "SeatPaymentReceiptEmail";
    }
}
