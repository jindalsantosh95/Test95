namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Constants
{
    public class Environment
    {

        public static readonly string CampaignUpdateResponseUrl = "CampaignUpdateResponseUrl";
        public static readonly string CampaignCreateResponseUrl = "CampaignCreateResponseUrl";
        public static readonly string PromotionUpdateResponseUrl = "PromotionUpdateResponseUrl";
        public static readonly string CampaignUnassignResponseUrl = "CampaignUnassignResponseUrl";
        public static readonly string PromotionCreateResponseUrl = "PromotionCreateResponseUrl";

        // Enviromental settings
        public static readonly string CosmosSqlDBConnectionMode = "CosmosSqlDBConnectionMode";

        // Transaction DB
        public static readonly string TransactionDBEndPoint = "TransactionDBEndPoint";
        public static readonly string TransactionDB = "TransactionDB";
        public static readonly string TransactionDBCollection = "TransactionCollection";

        // Service Bus
        public static readonly string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";
        public static readonly string CustomerServiceBusTopicName = "CustomerServiceBusTopicName";
        public static readonly string CustomerTranscationTopicName = "CustomerTranscationTopicName";
        public static readonly string LoyaltyMemberServiceBusTopicName = "LoyaltyMemberServiceBusTopicName";
        public static readonly string EmailTopicEndPoint = "EmailTriggerTopicName";

        // Command Status 
        public static readonly string CommandStatusLocation = "CommandStatusLocation";
        public static readonly string CommandStatusPollingDelayMS = "CommandStatusPollingDelayMS";
        public static readonly string CommandStatusPollingRetryCount = "CommandStatusPollingRetryCount";

        public static readonly string DatabaseID = "DatabaseID";
        public static readonly string SCVCommonRefDBConnString = "SCV:CommonRefDBConnString";
        public static readonly string ResponseDelayTimeInMS = "ResponseDelayTimeInMS";
        public static readonly string ResponsePollingCount = "ResponsePollingCount";

        #region Configuration for TargetedMembershipDoc method consume from Customer Transaction MS
        public static string OcpKeyHeader = "Ocp-Apim-Subscription-Key";
        public static readonly string CampaignManagementOcpKey = "CampaignManagement:OcpSubID";
        public static readonly string CampaignManagementAppId = "CampaignManagement:AppId";
        public static readonly string CampaignManagementClientSecret = "CampaignManagement:ClientSecret";
        public static readonly string AzureADEndpoint = "AzureADEndpoint";
        public static readonly string CustomerTxnMSResourceId = "CustomerTxnMS:ResourceId";
        public static readonly string CustomerTxnMSDetailsEndpoint = "CustomerTxnMSDetailsEndpoint";
        #endregion

        public static readonly string CampaignPromotionLinkResponseUrl = "CampaignPromotionLinkResponseUrl";
        public static readonly string CampaignPromotionDelinkResponseUrl = "CampaignPromotionDelinkResponseUrl";

        #region Configuration for MicroServiceDBKey from Customer Transaction MS
        public static readonly string CustomerTransactionMicroServiceDBKey = "CustomerTransactionMicroServiceDBKey";
        public static readonly string CustomerTransactionMicroServiceEventStoreTableName = "CustomerTransactionMicroServiceEventStoreTableName";
        public static readonly string CustomerTransactionMicroServiceResponseTableName = "CustomerTransactionMicroServiceResponseTableName";
        #endregion
        public static readonly string BlobConnStringForDownloads = "BlobConnStringForDownloads";
        public static readonly string ContainerNameForPromotions = "ContainerNameForPromotions";

        public static readonly string CustomerMSUrlGetbyCustID = "Customer:UrlGetbyCustID";
        public static readonly string CustomerMSUrlGetByMemNum = "Customer:UrlGetByMemNum";
        public static readonly string CustomerMSResourceId = "Customer:ResourceId";

        public static readonly string CommonCachingSlidingExpiration = "CommonCachingSlidingExpiration";
        public static readonly string CommonCachingAbsoluteExpiration = "CommonCachingAbsoluteExpiration";

        public static readonly string SCVReportingDBConnString = "SCV:ReportingDBConnString";
        public static readonly string ContainerNameForCampaigns = "ContainerNameForCampaigns";
        public static readonly string ContainerNameForCampAssocMembers = "ContainerNameForCampaignAssociatedMembers";
        public static readonly string MicroServiceDBKey = "MicroServiceDBKey";
    }
}