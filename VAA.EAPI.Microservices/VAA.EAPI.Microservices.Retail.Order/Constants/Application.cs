namespace VAA.EAPI.Microservices.Retail.Order.Constants
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
        public static readonly string DeleteType = "DELETE";
        public static readonly string MergeType = "MERGE";
        public static readonly string CacheKey = "ChannleCachekey";
        public static readonly string ChannelId = "ChannelId";
        public static readonly string ConfigTableName = "ChannelConfig";
        public const string UnEvenExchange = "Un-EvenExchange";
        public static readonly string SSRsPath = "Infrastructure\\Shared\\SSRsCodes.json";
        public const string ChildPtc = "CNN";
        public const string ChildMeal = "CHML";
        public const string AirlineCode = "VS";
        public const string SsrStatusCode = "HK";
        public const string Channel = "DFWEB";
        public const string OrderAddSsrSubject = "ManageSpecialService";
        public const string OrderAddSpecialRemarksSubject = "AddSpecialRemarks";
        public const string SpecialRemarkType = "SPECIAL";
        public const string VoucherRedemptionRemarkSubType = "VOUCHER_REDEMPTION";
        public const string OrderChangeServiceBusTopicName = "orderchangerequesteventmessage";


        #endregion


        #region Inflight Orders

        public static string CommonSQLRefDBMIToken;
        
        public static readonly string CarrierCacheKey = "InflightOrder_CARRIER_CACHE_KEY";
        public static readonly string AirportCacheKey = "InflightOrder_AIRPORT_CACHE_KEY";
        public const string WorkingProfileSqlQuery = "SELECT * FROM c WHERE c.PartitionKey  = @customerID and c.DocType = @DocType";
        public const string OrdersCollectionSqlQuery = "SELECT o.OrderID, o.OwnerAirline, o.DocType,o.OrderDetails.BookingReferences, o.OrderDetails.CreationDateTime, o.SegmentDetails, o.CustomerDetails FROM OrdersCollection o join cust in o.CustomerDetails WHERE ARRAY_CONTAINS(@OrderIDList, o.OrderID) and cust.FirstName = @firstName  and cust.LastName = @lastName";
        public const string FlightCollectionSqlQuery = "SELECT c.OrderID as PNRReference,c.FltDate as FlightDate FROM flightCollection c WHERE c.DocType = @DocType AND LEFT(c.OrderID,6) = @PNRNumber";
        public const string OrderAssociationEventSubject = "OrderAssociation";
        public const string OrdersCollQueryLoyaltyCustomers = "SELECT o.OrderID, o.OwnerAirline, o.DocType,o.OrderDetails.BookingReferences,o.OrderDetails.CreationDateTime, o.SegmentDetails, o.CustomerDetails FROM OrdersCollection o Join cust In o.CustomerDetails where cust.FrequentFlyer.AccountNumber = @LoyaltyNumber";
        public const string OrderAssocaitionEventHandlerTopicName = "orderAssociationRequesteventMessage";
        public const string OrderAssocaitionEventSubscriptionName = "OrderAssociationEventSubscriber";
        public static readonly string InflightOrdersExceptionFilePath = "Infrastructure\\Exceptions\\CustomExceptions\\ErrorConfig.json";
        public static readonly string _logApplicationInsightTimeFormat = "dd-MM-yyyy hh:mm:ss.fff tt";
        public static readonly string InflightExceptionFilePath = "Infrastructure\\Exceptions\\InflightOrders\\ErrorConfig.json";
        public static readonly string PartitionKey = "AirportLocation";

        #endregion

        #region OrderCreate

        public static readonly string ClientId = "client_id";
        public static readonly string ClientSecret = "client_secret";
        public static readonly string GrantType = "grant_type";
        public static readonly string Scope = "scope";
        public static readonly string Read = "read";
        public static readonly string ClientCredentials = "client_credentials";
        public const string MetaPartnerEventHandlerTopicName = "MetaPartnerEventMessage";
        public const string MetaPartnerEventSubscriptionName = "MetaPartnerEventSubscriber";
        public const string OrderEventHandlerTopicName = "OrderRequesteventMessage";
        public const string OrderEventSubscriptionName = "OrderEventSubscriber";
        public static readonly string OrderCreateEventSubject = "OrderCreate";
        public static readonly string OrderStoreEventSubject = "OrderStore";
        public static readonly string DeviceTypeText = "PDWDC";
        public static readonly string IpAddressText = "0.0.0.0";
        public static readonly string BrowserTypeText = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36";
        public static readonly string DocumentTypeCodeT = "T";
        public static readonly string ZuluDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
        public static readonly string OrderDocumentType = "OrderDocument";
        public static readonly string DCOMChannel = "DCOM";
        public static readonly string DeltaInfantPTC = "INF";
        public static readonly string DeltaAdultPTC = "ADT";
        public static readonly string DeltaYoungAdultPTC = "GBE";
        public static readonly string DeltaChildPTC = "CNN";
        public static readonly string UpgradePreferenceText = "NOU";
        public static readonly string ApplePay = "ApplePay";
        public static readonly string DlApplePay = "APPLE_PAY";
        public static readonly string DlPayPal = "MSPAY";
        public static readonly string DlCard = "CREDIT CARD";
        public static readonly string CCCONSENTOBTAINED = "CC_CONSENT_OBTAINED";
        public static readonly string ApplePayCode = "APAY";
        public static readonly string PayPalCode = "PYPL";
        public static readonly string RemarkText = "RXSA";
        public static readonly string CustomerOrderEnrollmentEventSubject = "Enrolment";
        public static readonly string EnrollmentChannel = "WEB";
        public static readonly string MediaCode = "NBF";
        public static readonly string DeltaProductCategoryIdFlights = "FLIGHTS";
        public static readonly string DeltaProductCategoryIdSeats = "SEATS";
        public static readonly string WarningDeltaProximity = "FE5180";
        public static readonly string WarningSEATNONCOMPLIANTPROXIMITY = "W003";
        public static readonly string WarningALLSEATFULFILMENTFAILED = "W001";
        public static readonly string WarningPARTIALSEATFULFILMENT = "W002";
        public static readonly string DlAwsOrderPath = "orders";
        public static readonly string DlAwsCallbackIdPath = "callback/{callbackId}";
        public static readonly string DlAwsCallbackInSecs = "{callbackInSecs}";
        public static readonly string PayWithMilesPartial = "PAY_WITH_MILES_PARTIAL";
        public static readonly string PayWithMilesFull = "PAY_WITH_MILES_FULL";
        public static readonly string InsurancePurchaseEventSubject = "InsurancePurchase";
        public static readonly string InsurancePurchaseEventType = "VAA.Order.Payment.InsurancePurchaseInitiate";
        public static readonly string InsuranceDecisionEventSubject = "InsuranceDecision";
        public static readonly string ClearPaymentDetailsEventSubject = "ClearPaymentDetails";
        public static readonly string MetaPartnerEventType = "VAA.Order.Meta.BookingNotification";
        public static readonly string MetaPartnerEventSubject = "MetaBookingNotification";

        #endregion
    }
}
