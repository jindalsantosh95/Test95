
namespace VAA.EAPI.Microservices.Loyalty.Vred.Constants
{

    public static class Application
    {

        public static string CommonSQLRefDBMIToken;
        // VRED OAuth credentials
        public static string AuthClientSecretCached;

        // OAuth Request Body Parameter Key Names
        public static readonly string GrantType = "grant_type";
        public static readonly string ClientCredentials = "client_credentials";
        public static readonly string ClientId = "client_id";
        public static readonly string ClientSecret = "client_secret";
        public static readonly string Audience = "audience";
        public static readonly string ApplicationxWwwFormUrlencoded = "application/x-www-form-urlencoded";

        // VRed OAuth access token property name in response
        public static string VRedAuthTokenCached;
        public static readonly string VRedAuthResponseAccessToken = "access_token";

        // VRed API 
        public static string VRedOcpApimSubscriptionKeyCached;
        public static readonly string AuthenticationScheme = "Bearer";
        public static readonly string OcpApimSubscriptionKeyName = "Ocp-Apim-Subscription-Key";
        public static readonly string ContenTypeapplicationjson = "application/json";

        // General HTTP Request settings
        public static readonly string ContentType = "Content-Type";
        public static readonly string CacheControl = "cache-control";
        public static readonly string NoCache = "no-cache";
        public static readonly string AcceptEncoding = "Accept-Encoding";
        public static readonly string Accept = "Accept";
        public static readonly string Everything = "*/*";
        public static readonly string GzipDeflate = "gzip, deflate";

        public static readonly string VRedDateFormat = "yyyy-MM-ddTHH:mm:ss";
        public static readonly string SCVDateFormat = "yyyy-MM-dd";

        public static readonly string DataVersion = "1.0";
        public static readonly string EventType = "com.virginatlantic.cloudevents.CustomerLoyaltyEvent";
        public static readonly string EventSubject = "VRED Sync Failure";
        public static readonly string SpecVersion = "1";
        public static readonly string EventGridSource = "EnrolmentAPI";

        public const string VredCustomerSyncEventHandlerTopicName = "enrolcustomer";
        public const string VredCustomerSyncEventSubscriptionName = "VredCustomerSyncSubscriber";
        public static readonly string WebMyaChannelID = "WEB-MYA";
        public static readonly string AppMyaChannelID = "App-MYA";
        public static readonly string PowerAppChannelId = "POWERAPP";
        public static readonly string VredEnableSignDate = "VredEnableSignDate";
        public static readonly string ISODateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";
        public static readonly string USCountryCode = "US";
        public static readonly string ParterUserId = "?partnerUserId=";
        public const string UpdateVredCustomerDetailsEventHandlerTopicName = "enrolcustomer";
        public const string UpdateVredCustomerDetailsEventSubscriptionName = "UpdateVredCustomerDetails";
        public static readonly string Operation = "STATUS";
        public static readonly string RequestId = "&requestId=";
        public const string ManagaeAccountUpdateTopicName = "manageaccount";
        public static readonly string CloudEventTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public const string ServiceBusTopicNameForVred = "vredeventmessages";
        public const string CancelVredTransactionsEventSubscriptionName = "CancelVredTransactionsSubscriber";
        public static readonly string HeaderParameterlocation = "location";
        public const string PartnerCode = "VAA";
        public const string CreditTransaction = "CREDIT";
        public const string DebitTransaction = "DEBIT";
        public const string EarnActivityType = "Earn";
        public const string BurnActivityType = "Burn";
        public const string AccrualType = "Accrual";
        public const string RedemptionType = "Redemption";
        public const string CancelStatus = "Cancelled";
        public static readonly string HeaaderParameterlocation = "location";
        public const string CreateVredTransactionsSubscriber = "CreateVredTransactionsSubscriber";
        public const string VRedRequestDateFormat = "MM/dd/yyyy";
        public const string iso8601DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        public const string TransactionAPIDateFormat = "yyyy-MM-dd";
        public const string VRedTxnOwner = "EAIUSER";
        public const string VRedTransactionActivitySubtype = "Product";
        public const string VRedActivityStatusProcessed = "Processed";
        public const string AccrualCancellation = "Accrual Cancellation";
        public const string RedemptionCancellation = "Redemption Cancellation";
        public const string VRedActivityStatusCancelled = "Cancelled";
        public static readonly string InsertTransactionAuditQuery = "EXEC[VGLCTXN].[uspSaveAuditInfo] @membershipNumber, @transactionID, @status, @transactionType, @activityDate, @transactionProcessedDate, @transactionSource, @serviceRequest, @siebelRequest, @siebelResponse, @createdBy, @createdByEmailId, @createdByUserName, @Result OUTPUT";
        public static readonly string UpdateTransactionAuditRequestQuery = "EXEC[VGLCTXN].[uspUpdateVredRequestAuditInfo] @transactionID, @vRedRequest, @Result OUTPUT";
        public static readonly string UpdateTransactionAuditReponseQuery = "EXEC[VGLCTXN].[uspUpdateVredResponseAuditInfo] @transactionID, @vRedResponse, @Result OUTPUT";
        public const string CreatedUser = "RetailTherapy";
        public const string ServiceBusTopicNameForVredMarketing = "enrolcustomer";
        public const string VredMarketingPreferenceEventSubscriptionName = "VredMarketingPreferenceSubscriber";
        public const string VredOneTrustDateFormat = "MM/dd/yyyy";
        public const string CanxReasonWithin24hr = "Within 24h of travel";
        public const string VoucherPointsProductName = "Voucher Points";
        public const string CanxReasonNoRefundOfPoints = "PPM - No refund of points";
        public const string TransactionPnrUpdateSubscriber = "UpdateVredTransactionPnrSubscriber";
    }
}
