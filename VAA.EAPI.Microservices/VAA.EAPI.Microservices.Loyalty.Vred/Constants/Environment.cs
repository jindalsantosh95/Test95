namespace VAA.EAPI.Microservices.Loyalty.Vred.Constants
{
    public static class Environment
    {
        // VRED OAuth credentials
        public static readonly string VRedAuthUrlSettingName = "VRedAuthUrl";
        public static readonly string VRedAuthAudienceSettingName = "AuthAudience";
        public static readonly string VRedAuthClientIdSettingName = "AuthClientId";
        public static readonly string VredAuthSecretSettingName = "AuthClientSecret";
        public static readonly string UpdateVredurlSettingName = "UpdateVRedAuthUrl";

        // VRed API 
        public static readonly string VRedOcpApimSubscriptionKeySettingName = "VRed-Ocp-Apim-Subscription-Key";
        public static string VRedOcpApimSubscriptionKeyCached;
        public static readonly string VRedEnrolmentAPISettingName = "VRedEnrolmentAPI";
        public static readonly string AuthenticationScheme = "Bearer";
        public static readonly string OcpApimSubscriptionKeyName = "Ocp-Apim-Subscription-Key";
        public static readonly string ContenTypeapplicationjson = "application/json";
        public static readonly string VRedEndpointUrl = "VRedEndpointUrl";
        public static readonly string VRedStatementEndpointUrl = "VRedStatementEndpointUrl";
        public static readonly string VRedCardDetailsEndpointUrl = "VRedCardDetailsEndpointUrl";
        public static readonly string VRedCardOcpApimSubscriptionKeyName = "VRedCard-Ocp-Apim-Subscription-Key";

        public static readonly string VRedConsentEndpointUrl = "VRedConsentEndpointUrl";
        public static readonly string VRedConsentRequestToken = "VRedConsentRequestToken";
        public static readonly string VRedConsentRequestKey1 = "VRedConsentRequestKey1";
        public static readonly string VRedConsentRequestKey2 = "VRedConsentRequestKey2";


        public static readonly string EventGridUrl = "EventGridUrl";
        public static readonly string EventGridTopicKey = "EventGridTopicKey";
        public static readonly string Source = "ServiceBusTopicName";
        public static readonly string MemberMetaDataSchema = "MemberMetaDataSchema";
        public static readonly string UrlVersion = "UrlVersion";
        public static readonly string CancelTransactionResponseUrl = "CancelTransactionResponseUrl";
        public static string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string VredTransactionAPIV2 = "VredTransactionAPIV2";
        public static readonly string VredTransactionAPICallbackUrl = "VredTransactionAPICallbackUrl";
        public static readonly string CreditMetaDataSchema = "CreditMetaDataSchema";
        public static readonly string DebitMetaDataSchema = "DebitMetaDataSchema";

        public const string ResponsePollingCount = "ResponsePollingCount";
        public const string ResponseDelayTimeInMS = "ResponseDelayTimeInMS";
        public const string MicroServiceDBKey = "MicroServiceDBKey";
        public const string MicroServiceResponseTableName = "MicroServiceResponseTableName";
        public static readonly string CreateTransactionResponseUrl = "CreateTransactionResponseUrl";
        public static readonly string VGLCDBConnString = "VGLCDBConnString";
        public static readonly string DatabaseID = "DatabaseID";
    }
}