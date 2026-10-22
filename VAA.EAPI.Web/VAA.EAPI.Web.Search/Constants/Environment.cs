namespace VAA.EAPI.Web.Search.Constants
{
    public class Environment
    {
        // Azure Active Directory
        public static readonly string AADAuthority = "AAD:Authority";
        public static readonly string AADTenant = "AAD:TenantId";
        public static readonly string AADV1Issuer = "AAD:TokenV1Issuer";

        // My Identity
        public static readonly string AppId = "MyIdentity:AppID";
        public static readonly string ClientSecret = "MyIdentity:ClientSecret";

        // Open API
        public static readonly string OpenApiDescription = "OpenApi__Description";
        public static readonly string OpenApiTokenUrl = "OpenApi__TokenUrl";
        public static readonly string OpenApiScope = "OpenApi__Scope";
        public static readonly string CommonCachingSlidingExpirationInDay = "CommonCachingSlidingExpirationInDay";
        public static readonly string CommonCachingAbsoluteExpirationInDay = "CommonCachingAbsoluteExpirationInDay";
        public static readonly string TimeToWaitBeforeNextRecall = "TimeToWaitBeforeNextRecall";

        // Postback URL
        public static readonly string PostbackUrl = "PostbackUrl";
        // GraphQL
        public static readonly string GraphQLUrl = "GraphQL:Url";
        public static readonly string GraphQLMyAccountUrl = "GraphQL:MyAccountUrl";
        public static readonly string GraphQLShowMockResponse = "GraphQL:ShowMockResponse";
        public static readonly string GraphQLBaggageSummaryMockResponse = "GraphQL:BaggageSummaryMockResponse";
        public static readonly string GraphQLSearchOffersPqId = "GraphQL:SearchOffersPqId";
        public static readonly string GraphQLSearchOffersNextPqId = "GraphQL:SearchOffersNextPqId";
        public static readonly string GraphQLRetrieveBasketPqId = "GraphQL:RetrieveBasketPqId";
        public static readonly string GraphQLFareConditionsPqId = "GraphQL:FareConditionsPqId";
        public static readonly string GraphQLMemberDetailsPqId = "GraphQL:MemberDetailsPqId";

        // Caching
        public static readonly string HttpClientTimeOut = "HttpClientTimeOut";
        public static readonly string RedisCacheConnectionString = "RedisCacheConnectionString";

        public static readonly string MicroServiceClientId = "MicroServiceClientId";
        public static readonly string MicroServiceClientSecret = "MicroServiceClientSecret";
        public static readonly string BasketUri = "BasketUri";
        public static readonly string OfferSubscriptionAutomationKey = "OfferSubscriptionAutomationKey";
        public static readonly string BasketSubscriptionAutomationKey = "BasketSubscriptionAutomationKey";
        public static readonly string OrderConfirmationMockResponse = "OrderConfirmationMockResponse";

        public static string BasketLink = "BasketLink";
        public static string ContactLink = "ContactLink";
        public static string TermsLink = "TermsLink";
        public static string PassengerLink = "PassengerLink";
        public static string BookCreateOrderPqId = "BookCreateOrderPqId";

        //Payment
        public static readonly string PaymentRefIdRequestMockResponse = "PaymentRefIdRequestMockResponse";
        public static string OrderPaymentRefIdPqId = "OrderPaymentRefIdPqId";
        public static string BffPaymentLink = "BffPaymentLink";
        public static string BffPaymentAmountLink = "BffPaymentAmountLink";
        public static string BffPaymentConfirmationLink = "BffPaymentConfirmationLink";

        public static readonly string GraphQLBrandPqId = "GraphQL:Brand:PqId";

        public static readonly string RedisConnectionString = "RedisCacheConnectionString";

        public static readonly string BookingCheckOutURL = "BookingCheckOutURL";

        public static readonly string GraphQLCarbonIndexPqId = "GraphQL:CarbonIndex:PqId";

    }
}
