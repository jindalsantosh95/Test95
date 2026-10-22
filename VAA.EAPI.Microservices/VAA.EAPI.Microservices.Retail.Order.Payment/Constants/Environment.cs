namespace VAA.EAPI.Microservices.Retail.Order.Payment.Constants
{
    public class Environment
    {
        public static readonly string OrderPaymentResponseUrl = "OrderPaymentResponseUrl";
        public static readonly string MockOrderPaymentService = "MockOrderPaymentService";
        public static readonly string EnableMultiplePayment = "EnableMultiplePayment";
        public static readonly string OpenApiDescription = "OpenApi__Description";
        public static readonly string HeaderParameterlocation = "location";
        public static readonly string HeaderRetryAfter = "Retry-After";
        public static readonly string HeaderPaymentStatusRetryAfterValue = "PaymentStatusRetryAfterValue";
        public static readonly string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string OrderServiceBusConnectionString = "OrderServiceBusConnectionString";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";
        public static readonly string MicroServiceDBKey = "MicroServiceDBKey";
        public static readonly string MicroServiceEventStoreTableName = "MicroServiceEventStoreTableName";
        public static readonly string MicroServiceResponseTableName = "MicroServiceResponseTableName";
        public static readonly string OpenApiTokenUrl = "OpenApi__TokenUrl";
        public static readonly string OpenApiScope = "OpenApi__Scope";
        public static readonly string DeltaMPIApiClientId = "DeltaMPIApiClientId";
        public static readonly string DeltaMPIApiClientSecret = "DeltaMPIApiClientSecret";
        public static readonly string DeltaMpiApiAuthority = "DeltaMpiApiAuthority";
        public static readonly string DeltaMpiApiResource = "DeltaMpiApiResource";
        public static readonly string DeltaMpiApiUrl = "DeltaMpiApiUrl";
        public static readonly string HttpClientTimeOutInMilliSeconds = "HttpClientTimeOutInMilliSeconds";
        public static readonly string Air4PaymentStagingEnvPar = "Air4PaymentStagingEnvPar";
        public static readonly string Air4HppPaymentLink = "Air4HppPaymentLink";
        public static readonly string DlPaymentAuthenticationUrl = "DlPaymentAuthenticationUrl";
        public static readonly string DlPaymentApiApplicationId = "DlPaymentApiApplicationId";
        public static readonly string DlPaymentApiChannelId = "DlPaymentApiChannelId";
        public static readonly string DlPaymentApiGenerateRefIdUri = "DlPaymentApiGenerateRefIdUri";
        public static readonly string DlPaymentClientId = "DlPaymentClientId";
        public static readonly string DlPaymentClientSecret = "DlPaymentClientSecret";
        public static readonly string DlPaymentPurchasePath = "DlPaymentPurchasePath";
        public static readonly string DlPaymentobjectName = "DlPaymentobjectName";
        public static readonly string DlPaymentApiGetPaymentDetailsUrl = "DlPaymentApiGetPaymentDetailsUrl";
        public static readonly string DlPaymentDetailMockEnabled = "DlPaymentDetailMockEnabled";
        public static readonly string DlPaymentApiFormsOfPaymentsUrl = "DlPaymentApiFormsOfPaymentsUrl";
        public static readonly string DlPaymentApiPurchaseInsuranceUri = "DlPaymentApiPurchaseInsuranceUri";
        public static readonly string DlPurchaseInsuranceUsername = "DlPurchaseInsuranceUsername";
        public static readonly string DlPurchaseInsurancePassword = "DlPurchaseInsurancePassword";
       
        public static readonly string PRIMockEnabled = "PRIMockEnabled";

        public static readonly string DlPaymentApiVoidUrl = "DlPaymentApiVoidUrl";
        public static readonly string MockVoidPaymentService = "MockVoidPaymentService";
        public static readonly string DlPaymentApiCaptureUrl = "DlPaymentApiCaptureUrl";

        public static readonly string MspFinancialInstAccountHolderName = "MspFinancialInstAccountHolderName";
        public static readonly string MspFinancialInstProviderId = "MspFinancialInstProviderId";
        public static readonly string MspFinancialInstReturnUrl = "MspFinancialInstReturnUrl";
        public static readonly string MspFinancialInstcountryCode = "MspFinancialInstcountryCode";

        public static readonly string DlGooglePay = "DlGooglePay";
        public static readonly string DlformsOfPaymentsGooglePay = "DlformsOfPaymentsGooglePay";

        public static readonly string DisableSeatIntegrationFlag = "DisableSeatIntegrationFlag";
        public static readonly string AllianzProviderName = "AllianzProviderName";

        public static readonly string DlPaymentApiClearDetailsUrl = "DlPaymentClearDetailsUrl";
        public static readonly string ServiceBusTopicClearPaymentDetails = "ServiceBusTopicClearPaymentDetails";
        public static readonly string EnableClearCVV = "EnableClearCVV";

        public static readonly string CjsCheckoutPaymentAppUrl = "CjsCheckoutPaymentAppUrl";
        public static readonly string CjsCheckoutPaymentAmountUrl = "CjsCheckoutPaymentAmountUrl";
        public static readonly string CjsCheckoutPaymentAppVersion = "CjsCheckoutPaymentAppVersion";
        public static readonly string MmbQueryLoginUrl = "MmbQueryLoginUrl";

        #region CPD
        public static readonly string CPDSaltkey = "CPDSaltkey";
        public static readonly string CPDAuthUsername = "CPDAuthUsername";
        public static readonly string CPDAuthPassword = "CPDAuthPassword";
        public static readonly string CPDInitializeRequestUrl = "CPDInitializeRequestUrl";
        public static readonly string CPDAuthorizeRequestUrl = "CPDAuthorizeRequestUrl";
        public static readonly string CPDCaptureRequestUrl = "CPDCaptureRequestUrl";
        public static readonly string CPDClientId = "CPDClientId";
        public static readonly string CPDAccount = "CPDAccount";
        public static readonly string CPDMotoAccount = "CPDMotoAccount";
        public static readonly string CPDRefundRequestUrl = "CPDRefundRequestUrl";
        public static readonly string PaymentCollection = "PaymentCollection";
        public static readonly string PaymentDatabaseName = "PaymentDatabaseName";
        public static readonly string PaymentDBEndpoint = "PaymentDBEndpoint";
        public static readonly string CpdBlobStorageUri = "CpdBlobStorageUri";
        public static readonly string CpdContainerName = "CpdContainerName";
        public static readonly string CpdCountryBlob = "CpdCountryBlob";
        public static readonly string CpdBlobStorageSaSKey = "CpdBlobStorageSaSKey";
        public static readonly string CPDGetTransactionStatusRequestUrl = "CPDGetTransactionStatusRequestUrl";
        public static readonly string GetTransactionStatusWaitTime = "GetTransactionStatusWaitTime";
        public static readonly string OrderDataMSEndpoint = "OrderDataMSEndpoint";
        public static readonly string OrderDataMSSubscriptionKey = "OrderDataMSSubscriptionKey";
        public static readonly string RetryCount = "RetryCount";
        public static readonly string BlobConnString = "BlobConnString";
        public static readonly string RpaNtdhContainerName = "RpaNtdhContainerName";
        public static readonly string GetPaymentDocQuery = "GetPaymentDocQuery";
        public static readonly string GetRefundPaymentDocQuery = "GetRefundPaymentDocQuery";
        public static readonly string CreatePaymentDocumentResponseUrl = "CreatePaymentDocumentResponseUrl";
        public const string ResponseDelayTimeInMS = "ResponseDelayTimeInMS";
        public const string ResponsePollingCount = "ResponsePollingCount";

        public static readonly string ARCMotoAccount = "ARCMotoAccount";
        public static readonly string ARCEcomAccount = "ARCEcomAccount";
        #endregion

        public static readonly string VredAuthenticationUrl = "VredAuthenticationUrl";
        public static readonly string VredApiClientId = "VredApiClientId";
        public static readonly string VredApiClientSecret = "VredApiClientSecret";
        public static readonly string VredEngagementUrl = "VredEngagementUrl";

        #region "Synchrony"
        public static readonly string SynchronyAuthenticationUrl = "SynchronyAuthenticationUrl";
        public static readonly string SynchronyApiClientId = "SynchronyApiClientId";
        public static readonly string SynchronyApiClientSecret = "SynchronyApiClientSecret";
        public static readonly string SynchronyDataShareAPIUrl = "SynchronyDataShareAPIUrl";
        public static readonly string SynchronyEncryptionPublicKey = "SynchronyEncryptionPublicKey";
        #endregion

        public static readonly string ReferenceDataMicroServiceDBKey = "ReferenceDataMicroServiceDBKey";
        public static readonly string CommonCachingSlidingExpirationInDay = "CommonCachingSlidingExpirationInDay";
        public static readonly string CommonCachingAbsoluteExpirationInDay = "CommonCachingAbsoluteExpirationInDay";
        public static readonly string MobileViewPaymentUrl = "MobileViewPaymentUrl";
    }
}
