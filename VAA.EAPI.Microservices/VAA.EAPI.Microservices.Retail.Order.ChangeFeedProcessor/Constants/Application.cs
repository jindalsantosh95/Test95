namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Constants
{
    public static class Application
    {
        public const string ISODateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";
        public const string SqlQuery = "SELECT * FROM CustomerOrderAssociation c WHERE c.PartitionKey = @DocType AND c.OrderID = @OrderID";
        public const string OrderDb = "OrderDB:SCVOrderDBName";
        public const string OrdersCollection = "OrdersCollection";
        public const string LeaseContainerName = "LeaseContainerName";
        public const string CosmosConnectionString = "CosmosConnectionString";
        public const int MaxItemsPerInvocation = 10;
        public const string EmailRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
        public const int TTLCalAdditionalDays = 185;
        public const string TTLCalInitialYearDigits = "20";
    }
}
