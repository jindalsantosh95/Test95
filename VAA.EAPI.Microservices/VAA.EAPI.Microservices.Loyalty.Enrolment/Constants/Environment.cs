namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Constants
{
    public class Environment
    {
        public static readonly string DefaultVAAMarketingPreferenceValue = "DefaultVAAMarketingPreferenceValue";
        public static readonly string DefaultVAPartnersMarketingPreferenceValue = "DefaultVAPartnersMarketingPreferenceValue";
        public static readonly string SCVCommonRefDBConnString = "SCV:CommonRefDBConnString";
        public static string ConnectionstringBlacklistedEmail = "BLobStorageConnectionString";
        public static string BlacklistedEmaiidcontainerName = "BlobStorageContainerName";
        public static readonly string ParentAgeLimit = "ParentAgeLimit";
        public static readonly string CustomerAgeLimit = "CustomerAgeLimit";
        public static readonly string DatabaseID = "DatabaseID";
        public static readonly string SCVCustDB = "SCV:CustDB";
        public static readonly string SCVCustDBCustColCntnr = "SCV:CustDB:CustColCntnr";
        public static readonly string SCVCustDBCustHstryColCntn = "SCV:CustDB:CustHstryColCntnr";
        public static readonly string SCVCustDBCustSearchColCntnr = "SCV:CustDB:CustSearchColCntnr";
        public static readonly string SCVCustDBEndPoint = "SCV:CustDB:EndPoint";
        public static readonly string SCVCustDBAuthKey = "SCV:CustDB:AuthKey";
        public static readonly string CosmosConnectionMode = "CosmosConnectionMode";
        public static readonly string FCNumberGenAPIURL = "FCNumberGenAPIURL";
        public static readonly string FCNumberGenAPIURLOCPKeyValue = "FCNumberGenAPIURLOCPKeyValue";
        public static readonly string FCNumberGenAPIClientID = "FCNumberGenAPIClientID";
        public static readonly string FCNumberGenAPIClientSecretValue = "FCNumberGenAPIClientSecretValue";
        public static readonly string FCNumberGenAPIResource = "FCNumberGenAPIResource";
        public static readonly string AzureADEndpoint = "AzureADEndpoint";
        public static readonly string EventType = "EventTopicType";
        public static readonly string EnrolmentEventType = "EnrolmentEventType";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";
        public static readonly string ApplicationInsightKey = "APPINSIGHTS_INSTRUMENTATIONKEY";
        public static readonly string EnrolmentResponseUrl = "EnrolmentResponseUrl";
        public const string ResponsePollingCount = "ResponsePollingCount";
        public const string ResponseDelayTimeInMS = "ResponseDelayTimeInMS";
        public static string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string VredCustomerAgeLimit = "VredCustomerAgeLimit";
        public static readonly string PowerAppChannelID = "PowerAppChannelID";
        public static readonly string DeltaChannelID = "DeltaChannelID";
        public static readonly string VredChannelId = "VredChannelID";
        public static readonly string WebSiteChannelId = "WebSiteChannelId";
        public static readonly string WebMyaChannelId = "WebMyaChannelId";
        public static readonly string BlacklistedEmailsFileName = "BlacklistedEmailsFileName";
        public static readonly string SCVCustTxnDBMmbrPrfleContnr = "SCV:CustTxnDB:MmbrPrfleContnr";
        public static readonly string SCVCustTxnDBEndPoint = "SCV:CustTxnDB:EndPoint";
        public static readonly string SCVCustTxnDB = "SCV:CustTxnDB";
        public static readonly string PostCodeMandatoryCountryList = "PostCodeMandatoryCountryList";
        public static readonly string EnrolmentAgeLimit = "EnrolmentAgeLimit";
    }
}
