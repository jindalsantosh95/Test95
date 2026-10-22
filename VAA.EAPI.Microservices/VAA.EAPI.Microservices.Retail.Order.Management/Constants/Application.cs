namespace VAA.EAPI.Microservices.Retail.Order.Management.Constants
{
    public static class Application
    {
        #region OrderChange

        public static readonly string OrderChangeEventSubject = "OrderChange";
        public static readonly string OrderChangeResponseUrl = "OrderChangeResponseUrl";
        public static readonly string TransactionId = "transactionId";
        public static readonly string AgentCityCode = "AgentCityCode";
        public static readonly string RepriceStoreAsBooked = "Y";
        public static readonly string SendEmailData = "Y";
        public static readonly string RequestOption2 = "R";
        public static readonly string EventSpecversion = "1.0";
        public static readonly string HeaaderParameterlocation = "location";
        public static readonly string ResponseThresholdInSec = "ResponseThresholdInSec";
        public static readonly string ResponseThreadDelayInSec = "ResponseThreadDelayInSec";
        public const string OrderChangeEventHandlerTopicName = "OrderChangeRequesteventMessage";
        public const string OrderChangeEventSubscriptionName = "OrderChangeEventSubscriber";
        public static readonly int PNRLength = 6;
        public static readonly string EvenExchange = "EVEN EXCHANGE";
        public static readonly string AirportLocation = "AirportLocation";
        public const string InfIdentificationChar = "I/";
        public const string InfSplitChar = "/";
        public const string InfantPtc = "INFT";
        public const string InfantRequestPtc = "INF";
        public static readonly string ErrorMessage = "ErrorMessage";
        public static readonly string ErrorCode = "ErrorCode";
        public static readonly string RedisplayFlag = "Y";
        public static readonly string VerifyDTI = "Y";
        public static readonly string DoNotDestroySessionFlag = "Y";
        public static readonly string VSCarrierCode = "VS";
        public static readonly string BusinessFault = "BusinessFault";
        public static readonly string SystemFault = "SystemFault";
        public const string FlightInformationEventHandlerTopicName = "flightinfotopic";
        public const string FlightInformationEventSubscriptionName = "flightinfosub";
        public static readonly string DeleteType = "DELETE";
        public static readonly string MergeType = "MERGE";
        public static readonly string CacheKey = "ChannleCachekey";
        public static readonly string ChannelId = "ChannelId";
        public static readonly string ConfigTableName = "ChannelConfig";
        public const string UnEvenExchange = "Un-EvenExchange";
        public static readonly string SSRsPath = "Infrastructure\\Shared\\SSRsCodes.json";
        public static string CommonSQLRefDBMIToken;
        public static readonly string CarrierCacheKey = "InflightOrder_CARRIER_CACHE_KEY";
        public static readonly string AirportCacheKey = "InflightOrder_AIRPORT_CACHE_KEY";
        public static readonly string PartitionKey = "AirportLocation";
        public const string ManageCustomerEventHandlerTopicName = "OrderChangeRequesteventMessage";
        public const string ManageCustomerEventSubscriptionName = "ManageCustomerEventSubscriber";
        public static readonly string ManageCustomerEventSubject = "ManageCustomer";
        public static readonly string ManageSpecialServiceEventSubject = "ManageSpecialService";
        public const string InvalidDOBMessage = "INVALID DATE OF BIRTH";
        public const string InvalidGenderMessage = "CK GENDER";
        public static readonly string YesIndicator = "Y";
        public static readonly string ManageCustomerDateFormat = "yyyy-MM-dd";
        public static readonly string Seat = "Seat";
        public const string OrderSeatEventHandlerTopicName = "orderseatfulfilmentresponseeventmessage";
        public const string OrderSeatEventSubscriptionName = "orderseatfulfilmentresponse";
        public const string OrderSeatDeleteEventHandlerTopicName = "orderseatdeletionresponseeventmessage";
        public const string OrderSeatDeleteEventSubscriptionName = "orderseatdeletionresponse";
        public const string ManageSpecialServiceEventHandlerTopicName = "OrderChangeRequesteventMessage";
        public const string ManageSpecialServiceEventSubscriptionName = "ManageSpecialServiceEventSubscriber";
        public const string AddSpecialRemarksEventSubscriptionName = "SpecialRemarksEventSubscriber";
        public const string VoucherRedemptionSource = "Online";

        #region Stamp FCNumber to PNR
        public const string AddSSRPassengersFlag = "N";
        public const string AddSSRSegmentsFlag = "Y";
        public const string AddSSRProductType = "SSREnum";
        public const string AddSSRRequestCode = "FQTV";
        public const string AddSSRStatusCode = "SS";
        public const string SeatUptakePos = "LON";
        public static readonly string CloudEventTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public static readonly string StampingProcessInitiated = "Stamping process initiated successfully";
        #endregion

        #endregion
    }
}
