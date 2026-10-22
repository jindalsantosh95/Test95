namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Constants
{
    public class EnvironmentSettings
    {
        // Enviromental settings
        public static readonly string CosmosSqlDBConnectionMode = "CosmosSqlDBConnectionMode";

        // Transaction DB
        public static readonly string TransactionDBEndPoint = "TransactionDBEndPoint";
        public static readonly string TransactionDB = "TransactionDB";
        public static readonly string TransactionDBCollection = "TransactionCollection";

        // Service Bus
        public static readonly string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";
        public static readonly string CustomerTransactionEventTopicName = "CustomerTransactionEventTopicName";

        // Command Status 
        public static readonly string CommandStatusLocation = "CommandStatusLocation";
        public static readonly string CommandStatusPollingDelayMS = "CommandStatusPollingDelayMS";
        public static readonly string CommandStatusPollingRetryCount = "CommandStatusPollingRetryCount";
        public static readonly string DatabaseID = "DatabaseID";
        public static readonly string SCVCommonRefDBConnString = "SCV:CommonRefDBConnString";

        //InRule
        public static readonly string TierRuleSetApplicationName = "TierRuleSetApplicationName";
        public static readonly string TierRuleSetEntityName = "TierRuleSetEntityName";
        public static readonly string RuleAppPassword = "RuleAppPassword";
        public static readonly string RuleAppRepositoryServiceUri = "RuleAppRepositoryServiceUri";
        public static readonly string RuleAppUserName = "RuleAppUserName";
        public static readonly string InRuleAPIURL = "InRuleAPIURL";
        public static readonly string InRuleAPIHeader = "InRuleAPIHeader";
        public static readonly string InRuleDisruptionAPIURL = "InRuleDisruptionAPIURL";
        public static readonly string InRuleDisruptionAPIKey = "InRuleDisruptionAPIKey";
        public static readonly string CombinedRuleApplicationName = "CombinedRuleApplicationName";
        public static readonly string CombinedRuleRequestEntityName = "CombinedRuleRequestEntityName";

        public static readonly string CommonCachingSlidingExpiration = "CommonCachingSlidingExpiration";
        public static readonly string CommonCachingAbsoluteExpiration = "CommonCachingAbsoluteExpiration";

        public static readonly string EconomyClasses = "EconomyClasses";
        public static readonly string PremEconomyClasses = "PremEconomyClasses";
        public static readonly string UpperClasses = "UpperClasses";
    }
}
