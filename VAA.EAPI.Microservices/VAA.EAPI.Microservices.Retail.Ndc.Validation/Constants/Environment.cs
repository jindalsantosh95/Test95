namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Constants
{
    public class Environment
    {
        #region Security Service
        public static readonly string SecuritySvcClientId = "SecuritySvcClientId";
        public static readonly string SecuritySvcClientSecret = "SecuritySvcClientSecret";
        public static readonly string AzureADEndpoint = "AzureADEndpoint";
        public static readonly string SecuritySvcResouceId = "SecuritySvcResouceId";
        public static readonly string SecuritySvcEndPoint = "SecuritySvcEndPoint";
        public static readonly string SecuritySvcOCPKey = "SecuritySvcOCPKey";
        public static string OcpKeyHeader = "Ocp-Apim-Subscription-Key";
        public static readonly string CallSecuritySvcFlag = "CallSecuritySvcFlag";
        public static readonly string SecurityMSHeartBeatUrl = "SecurityMSHeartBeatUrl";
        #endregion

        #region NDC Connect DB 
        public static readonly string NDCConnectDBConnectionString = "NDCConnectDBConnectionString";
        public static readonly string NDCConnectStoredProc = "NDCConnectStoredProc";
        public static readonly string NDCConnectAgencyOCNStoredProc = "NDCConnectAgencyOCNStoredProc";
        #endregion

        #region WebCore endpoint details
        public static readonly string AgentTktAuthorityEndpointGet = "AgentTktAuthorityEndpointGet";
        public static readonly string AgentTktAuthorityEndpointUpdate = "AgentTktAuthorityEndpointUpdate";
        public static readonly string AgentTktAuthorityEnv = "AgentTktAuthorityEnv";
        public static readonly string AgentTktAuthorityClientId = "AgentTktAuthorityClientId";
        public static readonly string AgentTktAuthoritySecretKey = "AgentTktAuthoritySecretKey";
        public static readonly string AgentTktAuthorityCacheExpiryDuration = "AgentTktAuthorityCacheExpiryDuration";
        #endregion

        #region Redis cache
        public static readonly string RedisConnectionString = "RedisConnectionString";
        public static readonly string AgencySlidingExpirationInDay = "AgencySlidingExpirationInDay";
        public static readonly string AgencyAbsoluteExpirationInDay = "AgencyAbsoluteExpirationInDay";
        public static readonly string SubscriptionCachingSlidingExpirationInDay = "SubscriptionCachingSlidingExpirationInDay";
        public static readonly string SubscriptionCachingAbsoluteExpirationInDay = "SubscriptionCachingAbsoluteExpirationInDay";
        #endregion

        #region IFG
        public static readonly string IFGAgentFopLookupUsername = "IFGAgentFopLookupUsername";
        public static readonly string IFGAgentFopLookupPassword = "IFGAgentFopLookupPassword";
        public static readonly string IFGAgentFopLookupUrl = "IFGAgentFopLookupUrl";
        #endregion
    }
}
