namespace VAA.EAPI.Microservices.Insights.Constants
{
    public static class Environment
    {
        public static readonly string AzureAdInstance = "AzureAd:Instance";
        public static readonly string AzureAdTenantId = "AzureAd:TenantId";

        public static readonly string ApimSubscriptionApiClientId = "ApimSubscriptionApiClientId";
        public static readonly string ApimSubscriptionApiClientSecret = "ApimSubscriptionApiClientSecret";
        public static readonly string ApimSubscriptionApiResource = "ApimSubscriptionApiResource";
        public static readonly string ApimSubscriptionId = "ApimSubScriptionId";
        public static readonly string ApimApiVersion = "ApimApiVersion";
        public static readonly string ApimSubscriptionListUri = "{0}/subscriptions/{1}/providers/Microsoft.ApiManagement/service?api-version={2}";
        public static readonly string ApimHostName = "ApimHostName";
        public static readonly string ApimEnvironment = "ApimEnvironment";
        public static readonly string HealthCheckSubscriptionKey = "HealthCheckSubscriptionKey";
        public static readonly string ApimHealthCheckAuthClientId = "ApimHealthCheckAuthClientId";
        public static readonly string ApimHealthCheckAuthClientSecret = "ApimHealthCheckAuthClientSecret";
        public static readonly string HealthCheckAuthClientId = "HealthCheckAuthClientId";
        public static readonly string HealthCheckAuthClientSecret = "HealthCheckAuthClientSecret";
        public static readonly string EmailAuthClientId = "EmailAuthClientId";
        public static readonly string EmailAuthClientSecret = "EmailAuthClientSecret";
        public static readonly string CustomerMicroserviceResourceId = "CustomerMicroserviceResourceId";
        public static readonly string EmailMicroserviceResourceId = "EmailMicroserviceResourceId";
        public static readonly string IdentityMicroserviceResourceId = "IdentityMicroserviceResourceId";
        public static readonly string InRuleMicroserviceResourceId = "InRuleMicroserviceResourceId";
        public static readonly string VredMicroserviceResourceId = "VredMicroserviceResourceId";
        public static readonly string CampaignManagementMicroserviceResourceId = "CampaignManagementMicroserviceResourceId";
        public static readonly string EnrolmentMicroserviceResourceId = "EnrolmentMicroserviceResourceId";
        public static readonly string CrmMicroserviceResourceId = "CrmMicroserviceResourceId";

        public static readonly string DevOpsPAT = "DevOpsPAT";
        public static readonly string DevOpsBaseUrl = "DevOpsBaseUrl";
        public static readonly string DevOpsProject = "DevOpsProject";
        public static readonly string PipelineUrl = "{0}/virginatlantic/{1}/_apis/pipelines?api-version=7.1";
        public static readonly string PipelineRunsUrl = "{0}/virginatlantic/{1}/_apis/build/builds?definitions={2}&branchName=refs/heads/main&api-version=7.1";

        public static readonly string DevOpsBuildUrl = "{0}/virginatlantic/{1}/_apis/build/builds/{2}/timeline?api-version=7.1";
        public static readonly string MetricsSubscriptionClientId = "MetricsSubscriptionClientId";
        public static readonly string MetricsSubscriptionClientSecret = "MetricsSubscriptionClientSecret";
        public static readonly string MetricsSubscriptionId = "MetricsSubscriptionId";
        public static readonly string MetricsRedisApiVersion = "MetricsRedisApiVersion";
        public static readonly string MetricsServiceBusApiVersion = "MetricsServiceBusApiVersion";
        public static readonly string MetricsInsightsVersion = "MetricsInsightsVersion";
        public static readonly string MetricsRedisListUri = "{0}/subscriptions/{1}/providers/Microsoft.Cache/redis?api-version={2}";
        public static readonly string MetricsServiceBusListUri = "{0}/subscriptions/{1}/providers/Microsoft.ServiceBus/namespaces?api-version={2}";
        public static readonly string DeploymentsKuduUri = "https://{0}.scm.azurewebsites.net/api/deployments/latest";

        public static readonly string ServicePlanBaseUrl = "https://management.azure.com/subscriptions/{0}/resourceGroups/rg-va-p-eun-enterprisems/providers/Microsoft.Web/serverfarms/{1}/providers/Microsoft.Insights/metrics?api-version=2018-01-01&metricnames=CpuPercentage%2CMemoryPercentage&aggregation=Maximum,Average&timespan{2}";
        public static readonly string LoyaltyPlanName = "LoyaltyPlanName";
        public static readonly string LoyaltyPlanClient = "LoyaltyPlanClient";
        public static readonly string LoyaltyPlanClientSecret = "LoyaltyPlanClientSecret";
        public static readonly string BasketPlanName = "BasketPlanName";
        public static readonly string BasketPlanClient = "BasketPlanClient";
        public static readonly string BasketPlanClientSecret = "BasketPlanClientSecret";
        public static readonly string OrderPlanName = "OrderPlanName";
        public static readonly string OrderPlanClient = "OrderPlanClient";
        public static readonly string OrderPlanClientSecret = "OrderPlanClientSecret";
        public static readonly string OfferPlanName = "OfferPlanName";
        public static readonly string OfferPlanClient = "OfferPlanClient";
        public static readonly string OfferPlanClientSecret = "OfferPlanClientSecret";
        public static readonly string GraphPlanName = "GraphPlanName";
        public static readonly string GraphPlanClient = "GraphPlanClient";
        public static readonly string GraphPlanClientSecret = "GraphPlanClientSecret";
        public static readonly string APIMPlanName = "APIMPlanName";
        public static readonly string APIMPlanClient = "APIMPlanClient";
        public static readonly string APIMPlanClientSecret = "APIMPlanClientSecret";
        public static readonly string CosmosDbUrl = "https://management.azure.com/subscriptions/{0}/resourceGroups/RG-VA-P-EUN-SCVPlus/providers/Microsoft.DocumentDb/databaseAccounts/{1}/providers/Microsoft.Insights/metrics?api-version=2018-01-01&metricnames=NormalizedRUConsumption&timespan={2}&interval=PT1H";
        public static readonly string CosmosClientId = "CosmosClient";
        public static readonly string CosmosClientSecret = "CosmosClientSecret";


        public const string ADOPipelineServiceBus = "ADOPipelineServiceBus";

        public const string AppSettingsMonitorSnapshotStorageConnectionString = "AppSettingsMonitorSnapshotStorageConnectionString";
        public const string AppSettingsMonitorSnapshotStorageContainerName = "AppSettingsMonitorSnapshotStorageContainerName";
        public const string AppSettingsMonitorAppInsightsAppId = "AppSettingsMonitorAppInsightsAppId";
    }
}
