namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
{
    public class Environment
    {
        public static readonly string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";

        public static readonly string CommandStatusLocation = "CommandStatusLocation";
        public static readonly string CommandStatusPollingDelayMS = "CommandStatusPollingDelayMS";
        public static readonly string CommandStatusPollingRetryCount = "CommandStatusPollingRetryCount";

        public static readonly string DatabaseID = "DatabaseID";
        public static readonly string SCVCommonRefDBConnString = "SCV:CommonRefDBConnString";
        public static readonly string ResponseDelayTimeInMS = "ResponseDelayTimeInMS";
        public static readonly string ResponsePollingCount = "ResponsePollingCount";
        public static string OcpKeyHeader = "Ocp-Apim-Subscription-Key";
        public static readonly string LoyaltyMbrMgmtOcpKey = "LoyaltyMbrMgmt:OcpSubID";
        public static readonly string LoyaltyMbrMgmtAppId = "LoyaltyMbrMgmt:AppId";
        public static readonly string LoyaltyMbrMgmtClientSecret = "LoyaltyMbrMgmt:ClientSecret";
        public static readonly string AzureADEndpoint = "AzureADEndpoint";
        public static readonly string CustomerTxnMSResourceId = "CustomerTxnMS:ResourceId";
        public static readonly string CustomerTxnMSDetailsEndpoint = "CustomerTxnMSDetailsEndpoint";
        public static readonly string WingsAssessmentProductId = "WingsAssessmentProductId";
        public static readonly string MillionMilerProductId = "MillionMilerProductId";
        public static readonly string WingsAssessmentBatchSize = "WingsAssessmentBatchSize";
        public const string WingsMemberAssessmentTimerSchedule = "%WingsMemberAssessmentTimerSchedule%";

        public static readonly string EmailTopicEndPoint = "EmailTriggerTopicName";
        public static readonly string SCVReportingDBConnString = "SCV:ReportingDBConnString";
        public static readonly string RuleEngineTopicName = "RuleEngineTopicName";
        public static readonly string BirthdayEmailTemplate = "BirthdayEmailTemplate";
        public static readonly string CommonCachingSlidingExpiration = "CommonCachingSlidingExpiration";
        public static readonly string CommonCachingAbsoluteExpiration = "CommonCachingAbsoluteExpiration";
        public static readonly string BlobConnStringForEnrolledMemberToPromotion = "BlobConnStringForEnrolledMemberToPromotion";
        public static readonly string ContainerNameForEnrolledMemberToPromotion = "ContainerNameForEnrolledMemberToPromotion";

        #region Configuration for MicroServiceDBKey from Customer Transaction MS
        public static readonly string CustomerTransactionMicroServiceDBKey = "CustomerTransactionMicroServiceDBKey";
        public static readonly string CustomerTransactionMicroServiceEventStoreTableName = "CustomerTransactionMicroServiceEventStoreTableName";
        #endregion

        public static readonly string GroupDocType = "GroupDocType";
        public static readonly string HoHLinkHeadSilverEmalTemplate = "HoHLinkHeadSilverEmalTemplate";
        public static readonly string HoHLinkHeadRedEmalTemplate = "HoHLinkHeadRedEmalTemplate";
        public static readonly string HoHLinkChildSilverEmalTemplate = "HoHLinkChildSilverEmalTemplate";
        public static readonly string HoHLinkChildRedEmalTemplate = "HoHLinkChildRedEmalTemplate";
        public static readonly string YoungFlyerEnrolmentConfirmationEmailTemplate = "YoungFlyerEnrolmentConfirmationEmailTemplate";

        public static readonly string HoHDelinkHeadEmailTemplate = "HoHDelinkHeadEmailTemplate";

        public static readonly string CustomerMSAppId = "Customer:AppId";
        public static readonly string CustomerMSOcpKey = "Customer:OcpSubID";
        public static readonly string CustomerMSUrlGetbyCustID = "Customer:UrlGetbyCustID";
        public static readonly string CustomerMSUrlGetByMemNum = "Customer:UrlGetByMemNum";
        public static readonly string LTGAwardEmailTemplate = "LTGAwardEmailTemplate";
        public static readonly string LTGMMEmailTemplate = "LTGMMEmailTemplate";
        public static readonly string MMEmailTemplate = "MMEmailTemplate";
        public static readonly string CustomerTransactionTopicName = "CustomerTransactionTopicName";
        public static readonly string AuditEventTopicName = "AuditEventTopicName";

        public static readonly string HoHAddressUpdateEmailTemplate = "HoHAddressUpdateEmailTemplate";
        public static readonly string CustomerMSUrl = "CustomerMSUrl";
        public static readonly string IsManualRun = "IsManualRun";
        public static readonly string BirthdayDateForManualRun = "BirthdayDateForManualRun";
        public static readonly string OrderMgtManageSpecialServiceResponseUrl = "OrderMgt:ManageSpecialServiceResponseUrl";
        public static readonly string OrderMgtMSAppId = "OrderMgt:AppId";
        public static readonly string OrderMgtMSOcpKey = "OrderMgt:OcpSubID";
        public static readonly string AddSSRTopicEndPoint = "OrderMgt:ServiceBusTopicName";
        public static readonly string OrderMgtServiceBusConnectionString = "OrderMgt:ServiceBusConnectionString";
        public static readonly string ChannelIdToAddSSR = "ChannelIdToAddSSR";
        public static readonly string ContainerNameForAddSSRResponse = "ContainerNameForAddSSRResponse";
        public static readonly string ContainerNameForAddSSRFile = "ContainerNameForAddSSRFile";
        public static readonly string ArchiveContainerNameForAddSSRFile = "ArchiveContainerNameForAddSSRFile";
        public static readonly string FCStampingTableName = "FCStampingTableName";
        public static readonly string ContainerNameForStatementText = "ContainerNameForStatementText";
        public static readonly string ItemNameForStatementText = "ItemNameForStatementText";
        public static readonly string CodeSharePartnerForStatementText = "CodeSharePartnerForStatementText";

        public static readonly string BlobConnStringForBulkUpload = "BlobConnStringForBulkUpload";
        public static readonly string ContainerNameForBulkUploadInput = "ContainerNameForBulkUploadInput";
        public static readonly string ArchiveContainerNameForBulkUpload = "ArchiveContainerNameForBulkUpload";
        public static readonly string ContainerNameForBulkUploadOutput = "ContainerNameForBulkUploadOutput";
        public static readonly string BulkMemberUpdateResponseTableName = "BulkMemberUpdateResponseTable";
        public static readonly string LoyaltyMbrMgmtSasKey = "LoyaltyMbrMgmtSasKey";
        public static readonly string VRedMSSubscriptionKey = "VRedMSSubscriptionKey";
        public static readonly string UpdateVredCustomerDetailsResponseUrl = "UpdateVredCustomerDetailsResponseUrl";
        public static readonly string VredMSAppId = "Vred:AppId";

        //Customer Transaction MS
        public static readonly string CustomerTransactionMSSubscriptionKey = "CustomerTransactionMSSubscriptionKey";
        public static readonly string CustomerProfileUrl = "CustomerProfileUrl";
        public static readonly string StampfcnumbertopnrcontainerUrl = "StampfcnumbertopnrcontainerUrl";

        public const string RuleEngineUrl = "RuleEngine:Url";
        public const string RuleEngineAppId = "RuleEngine:AppId";
        public const string RuleEngineOcpSubscriptionId = "RuleEngine:OcpSubID";
        public const string CustomerTxnMSUrl = "CustomerTxnMS:Url";
        public const string VredServiceBusTopicName = "VredServiceBusTopicName";
    }
}
