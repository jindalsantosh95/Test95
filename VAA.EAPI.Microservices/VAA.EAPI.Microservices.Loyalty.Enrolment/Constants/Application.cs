namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Constants
{
    public static class Application
    {
        public static readonly string AuthenticationScheme = "Bearer";
        public static string CommonSQLRefDBMIToken;
        public static readonly string CountryCacheKey = "EAPI_COUNTRY_CACHE_KEY";
        public static readonly string TitleCacheKey = "EAPI_TITLE_CACHE_KEY";
        public static readonly string TitleMasterCacheKey = "EAPI_TITLE_MS_CACHE_KEY";
        public static readonly string MediaCodeCacheKey = "EAPI_MEDIACODE_CACHE_KEY";
        public static readonly string PartnerCodeCacheKey = "EAPI_PARTNERCODE_CACHE_KEY";
        public static readonly string PartnerStatusCacheKey = "EAPI_PARTNERSTATUS_CACHE_KEY";
        public static readonly string MetroAreaCacheKey = "EAPI_METROAREA_CACHE_KEY";
        public static readonly string TitleValueCacheKey = "EAPI_TITLEVALUE_CACHE_KEY";
        public static readonly string MatchStringSqlQuery = "SELECT * FROM c WHERE c.MatchString = @matchString";
        public static readonly string EmailIdSqlQuery = "SELECT * FROM c WHERE c.EmailAddress = @emailId";
        public static readonly string MatchStringAndEmailIdSqlQuery = "SELECT * FROM c WHERE c.EmailAddress = @emailId AND c.MatchString = @matchString";
        public static readonly string VredDupCheckSqlQuery = "SELECT * FROM c WHERE c.FirstName= @firstName AND c.LastName = @lastName AND c.DateOfBirth = @dateOfBirth AND c.AddressDetails[0].Province =@province";
        public static readonly string MatchStringDateFormat = "dd-MMM-yyyy";
        public static string FCNumberGenAPIURLOCPKeyValueCached;
        public static string FCNumberGenAPITokenGenerated;
        public static string FCNumberGenAPIClientIDCached;
        public static string FCNumberGenAPIClientSecretValueCached;
        public static readonly string AuthorizationTokenType = "Bearer";
        public static readonly string ApimSubscription = "Ocp-Apim-Subscription-Key";
        public static readonly string AuthorizationHeader = "Authorization";
        public static readonly string CreateCustomerEventSubject = "Enrolment";
        public static readonly string DataVersion = "0.1";
        public static readonly string HeaderParameterlocation = "location";
        public static readonly string EAPIEnrolmentDOBFormat = "yyyy-MM-dd";
        public const string EnrolmentEventHandlerTopicName = "enrolcustomer";
        public const string EnrolmentEventSubscriptionName = "EnrolmentEventSubscriber";
        public static readonly string SearchDbQuery = "SELECT c.CustomerId, c.TierCode, c.id FROM c WHERE c.id = @id and c.DocType = @docType";
    }
}

