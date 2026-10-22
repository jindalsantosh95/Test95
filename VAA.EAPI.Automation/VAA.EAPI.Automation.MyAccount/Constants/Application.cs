using VAA.EAPI.Automation.Core;

namespace VAA.EAPI.Automation.MyAccount.Constants
{
    public static class Application
    {
        public const string DocTypeCustomerSearch = "CustomerSearch";
        public const string DocTypeCustomer = "Customer";
        public const string DocTypeCustomerProfile = "CustomerProfile";
        public const string DocTypeMembershipChange = "MembershipChange";
        public const string DocTypePartnerMemberShip = "PartnerMembership";
        public const string DocTypeFlyingClubMember = "FlyingClubMember";
        public static string PreferenceType = "VAAMarketingPermissions";
        public static string PreferenceValue = "Y";
        public static string GlassTierCodeUpgrade = "Red";
        public static string VRedChannelId = "VRED";
        public static string PowerAppChannelId = "POWERAPP";
        public static string DeltaChannelId = "DELTA.COM";
        public static string AxisChannelId = "AXIS";
        public static string PDMChannelId = "PDM";
        public static string SnappChannelId = "SNAPP";
        public static string RetailChannelId = "RETAIL";
        public static string MyAccountChannelId = "WEB-MYA";
        public static string ActiveFlag = "Y";
        public static string GlassTierCode = "Glass";
        public static string WEBChannelId = "WEB";
        public static string SchemaVersion = "0.1";

        //Non Prod Cosmos DB Details
        public static readonly string CustomerCosmosDbSIT = "CustomerDB_Acc";
        public static readonly string CustomerCosmosDbUAT = "CustomerDB_UAT";
        public static readonly string CustomerTransactionCosmosDbSIT = "CustomerTransactionsDB_Acc";
        public static readonly string CustomerTransactionCosmosDbUAT = "CustomerTransactionsDB_UAT";
        public static readonly string CustomerCosmosCollection = "CustomerCollection";
        public static readonly string CustomerTransactionCosmosCollection = "CustomerDetailsCollection";
        public static readonly string CustomerSearchCosmosCollection = "CustomerSearch";
        public static readonly string AuditCosmosDbSIT = "LoyaltyAuditDB";
        public static readonly string AuditCosmosDbUAT = "LoyaltyAuditDB_UAT";
        public static readonly string AuditCosmosCollection = "AuditCollection";
        public static readonly string CustomerOrderAssociationCollection = "CustomerOrderAssociation";
        public static readonly string CustomerCosmosDbEndpoint = "https://avadeuncmdbcustomer.documents.azure.com:443/";
        public static readonly string CustomerTransactionCosmosDbEndpoint = "https://avadeuncmdbcustomertransactions.documents.azure.com:443/";
        public static readonly string AuditCosmosDbEndpoint = "https://avadeuncmdbaudit.documents.azure.com:443/";
        public const string AzureB2CGetMemberDetailsEndpoint = "https://graph.microsoft.com/v1.0/users?$select=Identities,givenName,surname,id,passwordPolicies,objectId,userPrincipalName,accountEnabled,extension_f144285dc2c14758b7eaa9753ca9df60_MemberType,extension_f144285dc2c14758b7eaa9753ca9df60_memberStatus&$filter=Identities/any(x:x/issuer eq 'avadeunloyaltyfcb2c.onmicrosoft.com' and issuerAssignedId eq '{searchParameter}')";

        public static readonly string[] Origins = ["LHR", "LGW", "MAN", "BOM", "DEL", "ROM", "BVA"];
        public static readonly string[] Destinations = ["NYC", "JFK", "SWF", "ISP", "JRB", "LGA"];
        public static readonly string[] GenderTypes = ["M", "F", "X", "U"];
        public static readonly string LoginPassword = Vault.Default.GetSecretValue("MYA-Password");
        public const string AmexPcmUniqueIdStorageTable = "CampaignUniqueIdTable";
        public const string AmexPcmCampaignCode = "AMEXPCM";
        public const string AmexPcmCampaignId = "1774439172626";
        public const string AmexPcmPromotionId = "1770280672822";
        public static readonly string[] VoucherTypes = ["Flying Club reward voucher - UK Reward credit card", "Flying Club reward voucher - UK Reward+ credit card",  "Flying Club reward voucher - Virgin Red US credit card", "Flying Club reward voucher - Gold renewal", "Flying Club reward voucher - Gold 2000 Tier Points", "Gold Lounge Pass"];
        public static readonly string[] VoucherProductIds = ["1-2MG5VE1", "1718881074393", "1718882825098", "1-1Z1D", "1-2A1P", "1-1ZKH"];

        public enum CustomerTrasactionDBDocTypes
        {
            FlyingClubMember,
            Transaction,
            MembershipChange,
            Award,
            TargetedMembership,
            CampaignEnrollment,
            Group,
            CustomerProfile,
            ContactNote,
            CreditCard
        }

        public static readonly string[] PhoneCountryCodes = ["1", "44", "91", "81", "61", "49", "33", "39", "55", "34"];

        public enum LoyaltyMemberStatus
        {
            //List as in dev code. For account updates Merges, ActivePending, PendingCancellation and RouteDeal will not work
            Active,
            Expired,
            Frozen,
            Inactive,
            Merged,
            Deceased,
            Suspended,
            Cancelled,
            ActivePending,
            Fraud,
            PendingCancellation,
            RouteDeal,
        }

        public enum CustomerType
        {
            Loyalty,
            Basic
        }

        public enum ValidationRequestType
        {
            SignIn,
            ResetPassword,
            SignUp
        }

    }
}
