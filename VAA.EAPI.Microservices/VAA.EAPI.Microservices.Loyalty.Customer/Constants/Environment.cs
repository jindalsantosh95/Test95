namespace VAA.EAPI.Microservices.Loyalty.Customer.Constants
{
    public class Environment
    {
        public static readonly string SCVCustDBEndPoint = "SCV:CustDB:EndPoint";
        public static readonly string SCVCustDBAuthKey = "SCV:CustDB:AuthKey";
        public static readonly string SCVCustTxnDBAuthKey = "SCV:CustTxnDB:AuthKey";
        public static readonly string SCVCustTxnDBEndPoint = "SCV:CustTxnDB:EndPoint";
        public static readonly string CosmosConnectionMode = "CosmosConnectionMode";
        public static readonly string ProductIdTopMember = "ProductIdTopMember";
        public static readonly string SCVCustDB = "SCV:CustDB";
        public static readonly string SCVCustDBCustColCntnr = "SCV:CustDB:CustColCntnr";
        public static readonly string SCVCustDBCustSearchColCntnr = "SCV:CustDB:CustSearchColCntnr";
        public static readonly string SCVCustTxnDB = "SCV:CustTxnDB";
        public static readonly string SCVCustTxnDBCustPrflCntnr = "SCV:CustTxnDB:CustPrflCntnr";
        public static readonly string SCVCustTxnDBMmbrPrfleContnr = "SCV:CustTxnDB:MmbrPrfleContnr";
        public static readonly string SCVCustTxnDBMmbrshpChgContnr = "SCV:CustTxnDB:MmbrshpChgContnr";
        public static readonly string SCVCustTxnDBPartnerMemContnr = "SCV:CustTxnDB:PartnerMemContnr";
        public static readonly string TransactionDB = "TransactionDB";
        public static readonly string TransactionDBCollection = "CustomerDetailsCollection";
        public static string RetrieveMemberApiClientId = "InflightProfile:RetrieveMemberAPIClientID";
        public static string RetrieveMemberApiClientSecret = "InflightProfile:RetrieveMemberAPIClientSecret";
        public static string RetrieveMemberApiAuthority = "InflightProfile:RetrieveMemberAPIAuthority";
        public static string RetrieveMemberApiResource = "InflightProfile:RetrieveMemberAPIResource";
        public static string RetrieveMemberOcpKey = "InflightProfile:RetrieveMemberOCPKey";
        public static string RetrieveMemberDetailsEndpoint = "InflightProfile:RetrieveMemberDetailsEndpoint";
        public static string RetrieveLoyaltyMemberDetailsEndpoint = "LoyaltyProfile:RetrieveLoyaltyMemberDetailsEndpoint";
        public static readonly string EventTypeAddressLookup = "EventTopicTypeAddressLookup";
        public static readonly string AzureEventTopicEndPoint = "ServiceBusTopicName";
        public static readonly string EventTypeCustomerSync = "EventTopicTypeCustomerSync";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";
        public static readonly string SiebelEventGridUrl = "SiebelEventGridUrl";
        public static readonly string SiebelEventGridTopicKey = "SiebelEventGridTopicKey";
        public static readonly string Source = "SiebelEventSourceUrl";
        public static readonly string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string CosmosSqlDBConnectionMode = "CosmosSqlDBConnectionMode";
        public static readonly string VredChannelId = "VredChannelID";
        public static readonly string WebSiteChannelId = "WebSiteChannelId";

        public static readonly string DefaultVAAMarketingPreferenceValue = "DefaultVAAMarketingPreferenceValue";
        public static readonly string DefaultVAPartnersMarketingPreferenceValue = "DefaultVAPartnersMarketingPreferenceValue";
        public static readonly string SCVCommonRefDBConnString = "SCV:CommonRefDBConnString";
        public static string ConnectionstringBlacklistedEmail = "BLobStorageConnectionString";
        public static string BlacklistedEmaiidcontainerName = "BlobStorageContainerName";
        public static readonly string ParentAgeLimit = "ParentAgeLimit";
        public static readonly string CustomerAgeLimit = "CustomerAgeLimit";
        public static readonly string DatabaseID = "DatabaseID";
        public static readonly string FCNumberGenAPIURL = "FCNumberGenAPIURL";
        public static readonly string FCNumberGenAPIURLOCPKeyValue = "FCNumberGenAPIURLOCPKeyValue";
        public static readonly string FCNumberGenAPIClientID = "FCNumberGenAPIClientID";
        public static readonly string FCNumberGenAPIClientSecretValue = "FCNumberGenAPIClientSecretValue";
        public static readonly string FCNumberGenAPIResource = "FCNumberGenAPIResource";
        public static readonly string AzureADEndpoint = "AzureADEndpoint";
        public static readonly string EventType = "EventTopicType";
        public static readonly string EnrolmentEventType = "EnrolmentEventType";
        public static readonly string ApplicationInsightKey = "APPINSIGHTS_INSTRUMENTATIONKEY";
        public static readonly string EnrolmentResponseUrl = "EnrolmentResponseUrl";
        public const string ResponsePollingCount = "ResponsePollingCount";
        public const string ResponseDelayTimeInMS = "ResponseDelayTimeInMS";
        public static readonly string VredCustomerAgeLimit = "VredCustomerAgeLimit";
        public static readonly string PowerAppChannelID = "PowerAppChannelID";
        public static readonly string DeltaChannelID = "DeltaChannelID";
        public static readonly string WebMyaChannelId = "WebMyaChannelId";
        public static readonly string AppMyaChannelId = "AppMyaChannelId";
        public static readonly string PostCodeMandatoryCountryList = "PostCodeMandatoryCountryList";
        public static readonly string BlacklistedEmailsFileName = "BlacklistedEmailsFileName";
        public static readonly string RetryCount = "RetryCount";
        public static readonly string EnrolmentAgeLimit = "EnrolmentAgeLimit";
        public static string IsSearchOnCustSearch = "CustDB:IsCustSearchForTest";
        public static string SCVCustDBCustSearchColCntnrTest = "CustDB:CustSearchColCntnrForTest";
        public static readonly string VredEnableSignDate = "VredEnableSignDate";
        public static readonly string HttpClientTimeOut = "HttpClientTimeOut";
        public static readonly string AddressUpdateTemplate = "AddressUpdateTemplate";
        public static readonly string CustMSUpdateResponseURL = "CustMSUpdateResponseURL";
        public static readonly string CustMSVaEnrolResponseURL = "CustMSVaEnrolResponseURL";

        // Audit
        public static readonly string AuditServiceBusTopicName = "AuditServiceBusTopicName";

        public static readonly string CustomerSearchCollection = "CustomerSearchCollection";
    }
}
