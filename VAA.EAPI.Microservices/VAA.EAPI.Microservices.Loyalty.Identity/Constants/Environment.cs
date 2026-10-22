namespace VAA.EAPI.Microservices.Loyalty.Identity.Constants
{
    public class Environment
    {
        public static readonly string GraphAPIClientId = "ClientId";
        public static readonly string GraphAPIUrl = "GraphAPIUrl";
        public static readonly string TenantIdB2C = "TenantIdB2C";
        public static readonly string B2CTenantId = "TenantId";
        public static readonly string AuthenticationUrl = "AuthenticationUrl";
        public static readonly string GraphApiUrlVersion = "GraphApiUrlVersion";
        public static readonly string GraphApiUrlProxyAddressVersion = "GraphApiUrlProxyAddressVersion";
        public static readonly string SignInEmailAddress = "SignInEmailAddress";
        public static readonly string SignInUserName = "SignInUserName";
        public static readonly string CustomMemberTypeAttribute = "CustomMemberTypeAttribute";
        public static readonly string CustomMemberStatusAttribute = "CustomMemberStatusAttribute";
        public static readonly string CustomSecurityQuestion1Attribute = "CustomSecurityQuestion1Attribute";
        public static readonly string CustomSecurityAnswer1Attribute = "CustomSecurityAnswer1Attribute";
        public static readonly string CustomSecurityQuestion2Attribute = "CustomSecurityQuestion2Attribute";
        public static readonly string CustomSecurityAnswer2Attribute = "CustomSecurityAnswer2Attribute";
        public static readonly string CustomCustomerIdAttribute = "CustomCustomerIdAttribute";
        public static readonly string ExtensionPrefix = "ExtensionPrefix";
        public static readonly string AccountEnabled = "AccountEnabled";
        public static readonly string OtherMails = "OtherMails";
        public static readonly string Surname = "Surname";
        public static readonly string DisplayName = "DisplayName";
        public static readonly string GivenName = "GivenName";
        public static readonly string PasswordProfile = "PasswordProfile";
        public static readonly string PasswordPolicies = "PasswordPolicies";
        public static readonly string SignInNames = "SignInNames";
        public static readonly string UserPrincipalName = "UserPrincipalName";
        public static readonly string MailNickName = "MailNickName";
        public static readonly string Mail = "Mail";
        public static readonly string Identities = "Identities";
        public static readonly string ValidationJwtKey = "ValidationJwtKey";
        public static readonly string OtpTokenExpiryTime = "OtpTokenExpiryTime";


        public static readonly string ApplicationId = "ApplicationId";
        public static readonly string DebugOptions = "DebugOptions";
        public static readonly string TestLab = "TestLab";
        public static readonly string PoolType = "PoolType";
        public static readonly string RoleIndicator = "RoleIndicator";
        public static readonly string SecurityId = "SecurityId";
        public static readonly string AgentCityCode = "AgentCityCode";
        public static readonly string OptionNumberForDisplayRequest = "OptionNumberForDisplayRequest";
        public static readonly string ProvideResidualValue = "ProvideResidualValue";
        public static readonly string TrustedSource = "TrustedSource";
        public static readonly string Air4PnrCommonEndPoint = "Air4PnrCommonEndPoint";
        public static readonly string TicketingProcessV3EndPoint = "TicketingProcessV3EndPoint";
        public static readonly string EmailTopicEndPoint = "EmailTriggerTopicName";
        public static string ActivationLink = "ActivationLink";
        public static string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string CustomUserActivationTimeExpirationAttribute = "CustomUserActivationTimeExpirationAttribute";
        public static readonly string CustomerSyncEndPoint = "CustomerSyncTopicName";
        public static readonly string IdentityAcquireTokenExpiryMinutes = "ActivationTokenExpirationLimit";
        public static readonly string AllowTestEmail = "AllowTestEmail";
        public static readonly string TestEmailId = "TestEmailId";
        public static string IdentityActivationTokenExpiryMinutes = "IdentityActivationTokenExpiryMinutes";
        public static string IdentityActivationTokenIssuer = "IdentityActivationTokenIssuer";
        public static string RetryCounter = "RetryCounter";
        public static readonly string ReferenceDataMicroServiceDBKey = "ReferenceDataMicroServiceDBKey";
        public static readonly string OTPGenerateTableName = "OTPGenerateTableName";
        public static readonly string RandomDigitMinValue = "RandomDigitMinValue";
        public static readonly string RandomDigitMaxValue = "RandomDigitMaxValue";
        public static readonly string ContinuationTokenRandomDigitMinValue = "ContinuationTokenRandomDigitMinValue";
        public static readonly string ContinuationTokenRandomDigitMaxValue = "ContinuationTokenRandomDigitMaxValue";
        public static readonly string B2CDomain = "B2CDomain";
        public const string IdentityEventHandlerTopicName = "IdentityEventHandlerTopicName";
        public const string IdentityEventSubscriptionName = "IdentityEventSubscriptionName";

        public static readonly string OTPDigitLength = "OTPDigitLength";
        public static readonly string OtpEntraDigitLength = "OtpEntraDigitLength";
        public static readonly string OTPExpiryTimeLimit = "OTPExpiryTimeLimit";
        public static readonly string OTPGenerationLimit = "OTPGenerationLimit";

        public static readonly string OtpExpiryTimeSpan = "OtpExpiryTimeSpan";
        public static readonly string OtpLimit = "OtpLimit";
        public static readonly string OTPCreation = "OTPCreation";

        public static readonly string WelcomeEmailTemplate = "WelcomeEmailTemplate";
        public static readonly string OTPEmailTemplate = "OTPEmailTemplate";
        public static readonly string OTPSignTemplate= "OTPSignTemplate";
        public static readonly string EmailUpdateTemplate = "EmailUpdateTemplate";
        public static readonly string VaAccEmailB2CLink = "VaAccEmailB2CLink";
        public static readonly string VaAccEmailTemplate = "VaAccEmailTemplate";

        public static readonly string EntraAPIurl = "https://{0}/{1}/{2}";
        public static readonly string EntraAPIScope = "EntraAPIScope";
        public static readonly string EntraClientId = "EntraClientId";
        public static readonly string EntraClientSecret = "EntraClientSecret";
        public static readonly string EntraAPISubDomain = "EntraAPISubDomain";
        public static readonly string EntraAPIurlDomain = "EntraAPIurlDomain";
        public static readonly string EntraTenantId = "EntraTenantId";
        public static readonly string EntraLogging = "EntraLogging";

        public static readonly string FlyingClubTokenIssuer = "FlyingClubToken:Issuer";
        public static readonly string FlyingClubTokenAudience = "FlyingClubToken:Audience";
        public static readonly string FlyingClubTokenAuthorizedParty = "FlyingClubToken:AuthorizedParty";
        public static readonly string FlyingClubTokenClaimType = "FlyingClubToken:FlyingClubClaimType";
        public static readonly string FlyingClubTokenLifetimeSeconds = "FlyingClubToken:LifetimeSeconds";
        public static readonly string FlyingClubTokenKeyId = "FlyingClubToken:KeyVaultKeyId";
        public static readonly string AccountLinkingTransactionClientId = "AccountLinking:TransactionAssertion:ClientId";
        public static readonly string AccountLinkingTransactionResource = "AccountLinking:TransactionAssertion:Resource";
        public static readonly string AccountLinkingTrustedCallerApplicationId = "AccountLinking:TrustedCallerApplicationId";
    }
}
