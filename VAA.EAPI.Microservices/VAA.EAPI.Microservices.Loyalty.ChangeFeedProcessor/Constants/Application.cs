namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Constants
{
    public static class Application
    {

        public const string AirportDetailsLookUpTimer = "0 0 0 * * *";
        public static string CommonSQLRefDBMIToken = "";
        public static readonly string MergeType = "MERGE";
        public static readonly string DeleteType = "DELETE";
        public static readonly string DateTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public static readonly string SchemaVersion = "0.1";
        public static readonly string SqlQuery = "SELECT c.CustomerId as CustomerId, c.id as id, c.PhoneNumber, c.PhoneCountryCode FROM CustomerSearch c WHERE c.PartitionKey = @DocType AND c.LoyaltyMemberNumber = @LoyaltyMemberNumber";
        public const string CustomerDb = "CustomerDB_Acc";
        public const string CustomerCollection = "CustomerCollection";
        public const string LeaseCollectionName = "LeaseCustomerSearch";
        public const string CosmosConnectionString = "CosmosConnectionString";
        public const int MaxItemsPerInvocation = 10;
        public static readonly string CustomerSearchSqlQuery = "SELECT * FROM CustomerSearch c WHERE c.PartitionKey = @PartitionKey AND c.LoyaltyMemberNumber = @LoyaltyMemberNumber";
    }
}

