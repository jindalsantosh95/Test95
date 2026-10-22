namespace VAA.EAPI.Microservices.Retail.Order.Payment.Constants
{
    public class Application
    {
        public static readonly string OrderPaymentUpdateEventSubject = "OrderPaymentUpdate";
        public static readonly string EventSpecversion = "1.0";
        public static readonly string ChannelCodePath = "Infrastructure\\ChannelConfig\\ChannelConfiguration.json";
        public static readonly string CardExpiryPattern = @"^(0[1-9]|1[0-2])\-?([0-9]{2})$";//MM-YY
        public static readonly string TransactionDateTimeIsoFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        public static readonly string VoidTransactionEventSubject = "VoidOrderUpdate";
        public static readonly string EventUtcTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public static readonly string CPDBusinessErrorEventSubject = "CPDBusinessError";
        public static readonly string ClientId = "client_id";
        public static readonly string ClientSecret = "client_secret";
        public static readonly string GrantType = "grant_type";
        public static readonly string ClientCredentials = "client_credentials";
        public static string DeltaMpiApiTokenCached = "DeltaMpiApiTokenCached";
        public static readonly string AllianzAccam = "F208690";
        public static readonly string ChannelId = "channelid";
        public static readonly string AirlineCode = "airlineCode";
        public static readonly string AdapterCode = "mobile";
        public static readonly string ChannelIdValue = "ndc";
        public static readonly string AirlineCodeValue = "vs";
        public static readonly string AdapterCodeValue = "mobile";
        public static readonly string Air4PaymentStagingEnvParName = "routeOption";
        public static readonly string HeaderAccept = "accept";
        public static readonly string HeaderCachePar = "cache-control";
        public static readonly string HeaderCacheParValue = "no-cache";
        public static readonly string MPITokenGenerationError = "Delta Save MPI Token Generation Failed";
        public static readonly string AdultPtc = "ADT";
        public static readonly string Revenue = "Revenue";
        public static readonly string Award = "Award";
        public static readonly string RecordLocator = "FLIGHT";
        public static readonly string FormOfPaymentId = "01";
        public static readonly string DlApplePay = "APPLEPAY";
        public static readonly string DlPayPal = "PAYPAL";
        public static readonly string DlformsOfPaymentsApplePay = "APPLE_PAY";
        public static readonly string DlformsOfPaymentsPayPal = "MSPAY";
        public static readonly string ApplePay = "APPLEPAY";
        public static readonly string PayPal = "PAYPAL";
        public static readonly string InsurancePurchaseResponseEventType = "VAA.Order.Payment.InsurancePurchaseResponse";
        public static readonly string InsuranceProductCode = "INSURANCE";
        public const string OrderInsurancePurchaseMessage = "orderinsurancepurchasemessage";
        public const string OrderInsuranceEventSubscriber = "orderinsuranceeventsubscriber";
        public const string ClearPaymentDetailsMessage = "clearpaymentdetailsmessage";
        public const string ClearPaymentDetailsSubscriber = "clearpaymentdetailseventsubscriber";
        public const string InsurancePurchaseStatusTopicName = "insurancepurchasestatuseventmessage";
        public static readonly string InsurancePurchaseResponseStatusEventType = "VAA.Order.Payment.InsurancePurchaseResponseStatus";
        public static readonly string ClearPaymentDetailsEventSubject = "ClearPaymentDetails";
        public static readonly string ClearCVVDetailsEventType = "VAA.Order.InitiateClearPaymentDetails";
        public static readonly string DfMobileChannel = "DFMOBILE";

        #region CPD
        public static readonly string ISODateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        public static readonly string Typeid = "1009";
        public static readonly string Language = "en";
        public static readonly string Sdkversion = "2.1.0";
        public static readonly string Version = "2.1.0";
        public static readonly string Platform = "HTML5";
        public static readonly string contentType = "Content-Type";
        public static readonly string contentTypeValue = "text/xml";
        public static readonly string CardTypeid = "8";
        public static readonly string Info3dSecureversion = "2.1.0";
        public static readonly string Algorithmid = "AES";
        public static readonly string CreatedBy = "EAPI-PAY-MS";
        public static readonly string DocType = "PaymentDoc";
        public static readonly string PaymentQuery = "SELECT * from c where c.PartitionKey = @PartitionKey and c.id = @Id";
        public static readonly string RefundQuery = "SELECT c.id from c  where c.PartitionKey like @PartitionKey and c.id like @Id AND c.id NOT LIKE '%Split%' order by c.createdOn Desc";
        public static readonly string NDCCancelPnrHandlerTopicName = "ndccancelpnreventmessage";
        public static readonly string ZuluDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
        public static readonly string[] BatchWiseCardType = {"AX", "TP"};
        public static readonly string CaptureSuccessCode = "2001100";
        public static readonly string CallbackAcceptedCode = "1990";
        public static readonly string CaptureFailureCode = "2011";
        public static readonly string RefundStatus = "Partial Refund";
        public static readonly string OcpKeyHeader = "Ocp-Apim-Subscription-Key";
        public static readonly string NtdhEventType = "VAA.Order.Payment.Ntdh";
        public static readonly string NtdhRefundEventType = "VAA.Order.Payment.Ntdh.Refund";
        public static readonly string NtdhEventSubject = "NDTH";
        public const string NtdhTopicName = "ntdheventmessage";
        public static readonly string Rxoi = "RXOI";
        public static readonly string NdthChannel = "POSNDC";
        public static readonly string Rxsa = "RXSA";
        public static readonly string paymentOrchestrationProvider = "CPD";
        public static readonly string paymentOrchestrationProviderBSP = "BSP";
        public static readonly string Rxpr = "RXPR";
        public static readonly string xmlContentType = "application/xml";
        public const string RpaNtdhSubscriber = "RpaNtdhSubscriber";
        public static readonly string SplitPnrPaymentDocQuery = "SELECT c as PaymentDocument FROM c WHERE c.OrderId = @OrderId";
        public static readonly string RefundDocumentQuery = "SELECT c.id from c  where c.OrderId = @OrderId order by c.createdOn Desc";
        public const string CashPaymentDocumentTopicName = "cashpaymentdocumenteventmessage";
        public const string CashPaymentDocumentSubscriber = "CashPaymentDocumentSubscriber";
        public static readonly string IFG = "IFG";
        public static readonly string CreatePaymentDocumentEventType = "VAA.Order.Payment.CreatePaymentDocument";
        public static readonly string CreatePaymentDocumentEventSubject = "CreatePaymentDocument";
        public static readonly string HeaderParameterlocation = "location";
        public const string CreatePaymentDocumentTopicName = "createpaymentdocumenteventmessage";
        public const string CreatePaymentDocumentSubscriber = "CreatePaymentDocumentSubscriber";
        #endregion

        #region "CoBrand"
        public static readonly string SynchronyTokenGenerateUrl = "/generate-token";
        public static readonly string SynchronyApplicationStatusUrl = "/applicationStatus/{token}";
        public static readonly string SynchronyClientCode = "VIRGINRED";
        public static readonly string SynchronyAcorId = "SynchronyAcorId";
        public static readonly string SynchronySiteCode = "SynchronySiteCode";

     
        public static readonly string ConfigTableName = "ChannelConfig";
        public static readonly string CacheKey = "ChannleCachekey";
        #endregion

    }
}
