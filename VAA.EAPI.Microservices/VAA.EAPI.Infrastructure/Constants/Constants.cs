using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace VAA.EAPI.Infrastructure.Constants
{
    public static class Constants
    {
        public static readonly string ContentType = "Content-Type";
        public static readonly string MediaType = "application/json; charset=utf-8";
        public static readonly string XmlMediaType = "application/xml";
        public static readonly string FunctionAppDirPath = System.IO.Path.GetFullPath(".");
        public static readonly string ErrorCodesPath = "ErrorCodes/ErrorCodes.json";
        public static readonly string WarningCodesPath = "Warnings/Warnings.json";
        public static readonly string MicroServiceDBKey = "MicroServiceDBKey";
        public static readonly string MicroServiceEventStoreTableName = "MicroServiceEventStoreTableName";
        public static readonly string MicroServiceResponseTableName = "MicroServiceResponseTableName";
        public static readonly string OpenApiAuthTokenUrl = "https://login.microsoft.com/{ADTenantId}/oauth2/v2.0/token";
        public static readonly string OpenApiScopes = "https://{TenantID}.onmicrosoft.com/{ApplicationID}/Scope";
        public static readonly string ResponseUrlParName = "location";
        public static readonly string KeyVaultUri = "KeyVaultUri";
        public static readonly string KeyVaultTenantID = "KeyVaultTenantId";
        public static readonly string KeyVaultClientID = "KeyVaultClientId";
        public static readonly string KeyVaultClientSecret = "KeyVaultClientSecret";
        public static readonly string DeltaCertificateName = "DeltaCertificateName";
        public static X509Certificate2 DlCertificate;
        public static readonly string CacheConnectionTimeout = "CacheConnectionTimeout";

        #region Authentication
        public static readonly string AzureAdB2CInstance = "AzureAdB2C:Instance";
        public static readonly string AzureAdB2CClientId = "AzureAdB2C:ClientId";
        public static readonly string AzureAdB2CSignUpSignInPolicyId = "AzureAdB2C:SignUpSignInPolicyId";
        public static readonly string AzureAdB2CTenantId = "AzureAdB2C:TenantId";
        public static readonly string AzureAdInstance = "AzureAd:Instance";
        public static readonly string AzureAdClientId = "AzureAd:ClientId";
        public static readonly string AzureAdTenantId = "AzureAd:TenantId";
        public static readonly string AzureAdTokenV1Issuer = "AzureAd:TokenV1Issuer";
        #endregion Authentication

        #region B2C Graph
        public static string GraphB2CApiClientId = "GraphB2CAPIClientID";
        public static string GraphB2CApiClientSecret = "GraphB2CAPIClientSecret";
        public static string GraphB2CApiAuthority = "GraphB2CAPIAuthority";
        public static string GraphB2CApiResource = "GraphB2CAPIResource";
        public static string GraphB2COcpKey = "GraphB2COCPKey";
        public static string GraphB2CDetailsEndpoint = "GraphB2CDetailsEndpoint";
        #endregion B2C Graph

        #region Cosmos DB
        public static string CosmosSqlDBConnectionMode = "CosmosSqlDBConnectionMode";
        public static string CosmosGraphHostEndPoint = "CosmosGraphDBHost";
        public static string CosmosGraphDatabaseName = "CosmosGraphDatabaseName";
        public static string CosmosGraphKey = "CosmosGraphKey";
        public static string CosmosGraphDBContainer = "CosmosGraphDBContainer";
        public static string CosmosGraphEnableSSL = "CosmosGraphEnableSSL";
        public static string CosmosGraphPort = "CosmosGraphPort";
        #endregion Cosmos DB

        #region Security Headers
        public static readonly Dictionary<string, string> SecurityHeaders = new Dictionary<string, string>()
            {
                {"X-Content-Type-Options", "nosniff"},
                {"Strict-Transport-Security", "max-age=31536000; includeSubDomains"},
                {"X-Frame-Options", "DENY"},
            };
        #endregion Security Headers

        #region Exception coding
        public static readonly string TechnicalError = "G009";
        public static readonly string BadRequest = "400";
        public static readonly string Unauthorized = "401";
        public static readonly string Forbidden = "403";
        public static readonly string NotFound = "404";
        public static readonly string InvalidClaims = "G026";
        public static readonly string InvalidToken = "G001";
        public static readonly string GlobalExceptionFilePath = "GlobalException\\ErrorConfig.json";
        #endregion Exception coding

        #region Analytics

        public const string AnalyticsServiceBusTopicName = "analyticsTrackingMessages";
        public const string AnalyticsServiceBusSubscriptionName = "analyticsTrackingMessageSubscriber";
        public const string AnalyticsServiceBusConnectionString = "Analytics:ServiceBus:ConnectionString";

        #endregion

        #region ValidationMS
        public static readonly string KeyTypeDirect = "DIRECT";
        public static readonly string KeyTypeIndirect = "INDIRECT";
        #endregion


    }
}
