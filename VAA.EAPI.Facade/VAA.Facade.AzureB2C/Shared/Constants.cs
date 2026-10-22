namespace VAA.Facade.AzureB2C.Shared
{ 
    public static class Constants
    {
       
        public static readonly string AzureADEndpoint = "AzureADEndpoint";
        public static readonly string azureADEndpoint = "AzureADEndpoint";
        public static readonly string EnrolmentResource = "EnrolmentResource";
        public static readonly string EnrolmentApiBaseURL = "EnrolmentApiBaseURL";
        public static readonly string EnrolmentApiURI = "EnrolmentApiURI";
        public static readonly string apimSubscription = "Ocp-Apim-Subscription-Key";
        public static readonly string authorizationHeader = "Authorization";

        public static string EnrolmentSubscriptionNameValue = "EnrolmentSubscriptionNameValue";
        public static string EnrolmentClientID = "EnrolmentClientID";
        public static string EnrolmentClientSecretValue = "EnrolmentClientSecretValue";
        public static string EnrolmentSubscriptionNameValueCached;
        public static string EnrolmentClientIDCached;
        public static string EnrolmentClientSecretValueCached;
        public static string EnrolmentAPITokenGenerated;

        public const string BasicCustomerType = "Basic";
        public const string EmailUsageType = "Personnel";
        public const string MarketingPreferences = "VAAMarketingPermissions";
        public const string B2CAppId = "B2CAppId";
        public const string AzureCertificateThumbprint = "AzureCertificateThumbprint";
        public const string ContentType = "ContentType";
        public const string MediaType = "application/json";
        public const string EnrolmentChannelId = "RETAIL";
        public const string B2CActionContinue = "Continue";
        public const string B2CActionBlockPage = "ShowBlockPage";
        public const string B2CActionValidation = "ValidationError";
        public const string B2CResponseApiVersion = "1.0.0";
        public const string B2CStepPostAttributeCollection = "PostAttributeCollection";

        public static string FunctionAppDirPath;



        public static string _functionAppDirPath;
        public static readonly string _authenticationScheme = "Bearer";
        public static readonly string _logApplicationInsightTimeFormat = "dd-MM-yyyy hh:mm:ss.fff tt";

        #region error codes
        public static readonly string TechnicalError = "G009";
        public static readonly string _unauthorized = "401";

        public static readonly string InvalidB2CToken = "G001";
        #endregion error codes


    }
}