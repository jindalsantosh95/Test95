namespace VAA.Facade.EnrolmentFacade.Constants
{
    public static class Constants
    {    
        public static readonly string AuthenticationScheme = "Bearer";
        public static readonly string RetrieveEnrolmentApiClientId = "RetrieveEnrolmentApiClientId";
        public static readonly string RetrieveEnrolmentApiClientSecret = "RetrieveEnrolmentApiClientSecret";
        public static readonly string RetrieveEnrolmentApiAuthority = "RetrieveEnrolmentApiAuthority";
        public static readonly string RetrieveCustomerApiResource = "RetrieveCustomerApiResource";
        public static readonly string RetrieveEnrolmentOcpKey = "RetrieveEnrolmentOcpKey";
        public static readonly string RetrieveCustomerEndpoint = "RetrieveCustomerEndpoint";       
        public static readonly string AzureEventTopicEndPoint = "AzureEventTopicEndPoint";
        public static readonly string AzureEventTopicKey = "AzureEventTopicKey";
        public static readonly string ApplicationInsightKey = "APPINSIGHTS_INSTRUMENTATIONKEY";
        public static readonly string DataVersion = "0.1";
        public static readonly string CreateFacadeEventSubject = "EnrolmentSubject";
        public static readonly string EventSource = "Enrolment";
        public static readonly string EventType = "EventType";
        public static readonly string _contenTypeapplicationjson = "application/json";
        public static readonly string _cache_control = "cache-control";
        public static readonly string _no_cache = "no-cache";
        public static readonly string _accept_Encoding = "Accept-Encoding";
        public static readonly string _gzip_deflate = "gzip, deflate";
        public static readonly string _accept = "Accept";
        public static readonly string _everything = "*/*";
        public static readonly string ContentType = "ContentType";
        public static readonly string MediaType = "application/json";
        public static string FunctionAppDirPath;
        public static readonly string PnrLinkMutationQuery = "mutation accountPnrLinkEnrolmentMutation($request: CustomerDTOInput!) {accountPnrLinkEnrolment(request: $request)}";
        public static readonly string PnrLinkMutationEndpoint = "GraphQL:PnrLinkMutationEndpoint";


        #region Authentication
        public static readonly string AzureAdB2CInstance = "AzureAdB2C:Instance";
        public static readonly string AzureAdB2CClientId = "AzureAdB2C:ClientId";
        public static readonly string AzureAdB2CSignUpSignInPolicyId = "AzureAdB2C:SignUpSignInPolicyId";
        public static readonly string AzureAdB2CTenantId = "AzureAdB2C:TenantId";
        public static readonly string AzureAdInstance = "AzureAd:Instance";
        public static readonly string AzureAdClientId = "AzureAd:ClientId";
        public static readonly string AzureAdTenantId = "AzureAd:TenantId";
        public static readonly string AzureAdTokenV1Issuer = "AzureAd:TokenV1Issuer";
        #endregion Authentication

        public static readonly string TechnicalError = "G009";
        public static readonly string Unauthorized = "401";
        public static readonly string InvalidToken = "G001";
        public static readonly string GlobalExceptionFilePath = "Infrastructure\\GlobalException\\ErrorConfig.json";


    }
}
