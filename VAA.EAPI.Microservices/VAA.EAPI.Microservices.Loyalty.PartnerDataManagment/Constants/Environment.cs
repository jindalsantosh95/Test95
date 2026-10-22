namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants
{
    public class Environment
    {
        public const string clientIDFlightAPI = "ClientIDFlightAPI";
        public const string clientSecrectFlightAPI = "ClientSecrectFlightAPI";
        public const string codeShareFlightApiUrl = "CodeShareFlightApiUrl";
        public const string resourceIDMemberApi = "ResourceIDMemberApi";
        public const string subscriptionKeyMemberApi = "SubscriptionKeyMemberApi";
        public const string authUrlApi = "AuthUrlApi";
        public const string tenantNameApi = "TenantNameApi";
        public const string RetroDurationinDays = "RetroDurationinDays";
        public const string FlownSQLDBConnString = "FlownSQLDBConnString";
        public const string AutoRetroResetDuration = "AutoRetroResetDuration";
        public const string AutoRetroTimerChrone = "%AutoRetroTimerChrone%";
        public const string NonAccrualVSClasses = "NonAccrualVSClasses";
        public const string ServiceBusConnectionString = "ServiceBusConnectionString";
        public const string MicroServiceDBKey = "MicroServiceDBKey";
        public const string MicroServiceEventStoreTableName = "MicroServiceEventStoreTableName";
        public const string OrderMSServiceBusConnectionString = "OrderMSServiceBusConnectionString";
        public const string FlownPassengerServiceBusConnectionString = "FlownPassengerServiceBusConnectionString";
        public const string ReferenceMicroServiceDBKey = "ReferenceMicroServiceDBKey";

        //SkyTeamKafka Settings
        public const string SkyTeamKafkaBootstrapServers = "SkyTeamKafkaBootstrapServers";
        public const string AccrualOutboundKafkaTopic = "AccrualOutboundKafkaTopic";
        public const string AccrualInboundKafkaTopic = "AccrualInboundKafkaTopic";
        public const string AutoAccrualFeedbackOutboundKafkaTopic = "AutoAccrualFeedbackOutboundKafkaTopic";
        public const string SkyTeamPgpPublicKey = "SkyTeamPgpPublicKey";
        public const string VsPgpPrivateKey = "VsPgpPrivateKey";
        public const string SkyTeamKafkaConsumerGroup = "SkyTeamKafkaConsumerGroup";
        public const string AutoAccrualFeedbackOutboundKafkaConsumerGroup = "AutoAccrualFeedbackOutboundKafkaConsumerGroup";
        public const string AccrualPgpEncryptionKeyId = "AccrualPgpEncryptionKeyId";

        public const string AutoAccrualFallbackSchedule = "%AutoAccrualFallbackSchedule%";
        public const string AutoAccrualFallbackDepartureThresholdDays = "AutoAccrualFallbackDepartureThresholdDays";
        public const string SkyTeamPartnerCacheExpiryHours = "SkyTeamPartnerCacheExpiryHours";

        //SkyTeam Inbound Auto Accrual
        public const string SkyTeamInboundDataTableName = "SkyTeamInboundDataTableName";

        //VS Retro Claim Input Validation
        public const string VSClaimInputValidationBlobStorageUri = "VSClaimInputValidationBlobStorageUri";
        public const string VSClaimInputValidationBlobContainerName = "VSClaimInputValidationBlobContainerName";
        public const string VSClaimInputValidationBlobSasKey = "VSClaimInputValidationBlobSasKey";

        public const string SkyTeamInboundAccrualMessageLookupTableName = "SkyTeamInboundAccrualMessageLookupTableName";
        public const string LoyaltyMemberManagementTopicName = "LoyaltyMemberManagementTopicName";
    }
}
