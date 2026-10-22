using VAA.EAPI.Automation.Core;

namespace VAA.EAPI.Automation.MarketingLoyalty.Constants
{
    public static class Application
    {
        public static readonly string CloudEventTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public static readonly string SpecVersion = "1.0";
        public static readonly string DataContentType = "application/json";
        public static readonly string EventSpecversion = "1.0";
        public static readonly string ISODateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";
        public const string EmailInboxForValidations = "EAPICICDTest@fly.virgin.com";
        public const string BirthdayBonusCombinedRuleSetEvent = "VAA.CustomerTxns.RuleEngine.CombinedRuleSetEvent";
        public const string DissolveHouseholdAccounts = "VAA.Loyalty.MemberMgmt.DelinkHouseholdAccounts";
        public const string LTGAwardEvent = "VAA.Loyalty.Customer.LTGAwardEvent";
        public const string MillionMilerAwardEvent = "VAA.Loyalty.Customer.MillionMilerAwardEvent";
        public const string CampaignRegsiterEvent = "VAA.PowerApp.CampaignMgmt.CampaignRegisterEvent";
        public const string DissolveHouseholdAccountOnStatusUpdate = "VAA.Loyalty.Customer.MemberStatus";
        public const string DissolveSuppSilverAccountOnTierDowngrade = "VAA.Loyalty.Customer.MemberTierUpdateEvent";
        public const string ServiceBusTopicNameRuleEngine = "ruleengineeventmessages";
        public const string ServiceBusTopicNameLoyaltyMS = "loyaltymembermanagementeventmessages";
        public const string ServiceBusTopicNameCampaignMS = "campaignmanagementeventmessages";
        //SnowFlakeConnectivity Constants        
        public const string SnowFlakeAuthenticator = "snowflake_jwt";
        public const string SnowFlakeUser = "PROD_VAA_AZURE_SERVICE";
        public const string SnowFlakeRole = "PROD_AZURE_SERVICE";
        public const string SnowFlakeDataBase = "PROD_VIRGIN_ATLANTIC";
        public const string SnowFlakeSchema = "VAA_EXPORT";
        public const string SnowFlakeWarehouse = "PROD_AZURE_SERVICE";
        //Non Prod Cosmos DB Details
        public static readonly string CustomerCosmosDbSIT = "CustomerDB_Acc";
        public static readonly string CustomerCosmosDbUAT = "CustomerDB_UAT";
        public static readonly string CustomerTransactionCosmosDbSIT = "CustomerTransactionsDB_Acc";
        public static readonly string CustomerTransactionCosmosDbUAT = "CustomerTransactionsDB_UAT";
        public static readonly string CustomerCosmosCollection = "CustomerCollection";
        public static readonly string CustomerTransactionCosmosCollection = "CustomerDetailsCollection";
        public static readonly string AuditCosmosDbSIT = "LoyaltyAuditDB";
        public static readonly string AuditCosmosDbUAT = "LoyaltyAuditDB_UAT";
        public static readonly string AuditCosmosCollection = "AuditCollection";
        public static readonly string CustomerOrderAssociationCollection = "CustomerOrderAssociation";
        public static readonly string CustomerCosmosDbEndpoint = "https://avadeuncmdbcustomer.documents.azure.com:443/";
        public static readonly string CustomerTransactionCosmosDbEndpoint = "https://avadeuncmdbcustomertransactions.documents.azure.com:443/";
        public static readonly string AuditCosmosDbEndpoint = "https://avadeuncmdbaudit.documents.azure.com:443/";
        //Prod Cosmos DB Details
        public static readonly string CustomerCosmosDbProd = "CustomerDB";
        public static readonly string CustomerTransactionCosmosDbProd = "CustomerTransactionsDB";
        public static readonly string AuditCosmosDbProd = "LoyaltyAuditDB";
        public static readonly string SingleOrderViewCosmosDbProd = "SingleOrderViewDB";
        public static readonly string CustomerCosmosDbEndpointProd = "https://avapeuncmdbcustomer.documents.azure.com:443/";
        public static readonly string CustomerTransactionCosmosDbEndpointProd = "https://avapeuncmdbcustomertransactions.documents.azure.com:443/";
        public static readonly string AuditCosmosDbEndpointProd = "https://avapeuncmdbaudit.documents.azure.com:443/";
        public static readonly string SingleOrderViewCosmosDbEndpointProd = "https://avapeuncmdbsingleorderview.documents.azure.com:443/";
        public const string AutomationScriptCreatedBy = "LOOPSScript@fly.virgin.com";
        public static readonly string[] CabinClasses = ["Business", "Economy", "Premium", "First"];
        public static readonly string[] SkyTeamPartnerCodes = ["AF", "DL", "KL", "KQ", "ME", "SK"];
        public static readonly string[] Origins = ["LHR", "LGW", "MAN", "BOM", "DEL", "ROM", "BVA"];
        public static readonly string[] Destinations = ["NYC", "JFK", "SWF", "ISP", "JRB", "LGA"];
        public static readonly string[] PhoneCountryCodes = ["1", "44", "91", "81", "61", "49", "33", "39", "55", "34"];
        public const string FlightProduct = "Flight";
        public const string PowerAppsTransactionChannel = "PAUSER";
        public const string BirthdayBonusStoredProc = "[dbo].[SP_GET_MEMBERS_FOR_BIRTHDAY_BONUS]";
        public const string BirthdayBonusTransactionProductId = "1-29I7";
        public const string BirthdayBonusTransactionValue = "2000";
        public const string ProcessedTransactionStatusCode = "Processed";
        public static string LogInPassword = Vault.Default.GetSecretValue("FlyingClubLoginPassword");



        public enum CustomerTrasactionDBDocTypes
        {
            FlyingClubMember,
            Transaction,
            MembershipChange,
            Award,
            TargetedMembership,
            Group,
            CustomerProfile,
            ContactNote,
            CreditCard
        }

        public enum TransactionProduct
        {
            Flight_Redemption,
            Flight,
            Authorised_Points_Credit,
            Points_Misquote,
            Reward_voucher_redemption,
            Tier_Points_Adjustment,
            Voucher_Points
        }

    }
}
