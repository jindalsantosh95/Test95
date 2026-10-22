namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Constants
{
    public static class Environment
    {
        public static readonly string SCVOrderDBCollection = "OrderDB:SCVOrderDBCollection";
        public static readonly string SCVOrderDBEndPoint = "CosmosConnectionString";
        public static readonly string SCVOrderDBName = "OrderDB:SCVOrderDBName";
        public static readonly string LeaseContainerName = "LeaseContainerName";

        public static readonly string SingleOrderViewDBCollection = "SingleOrderViewDB:SingleOrderViewDBCollection";
        public static readonly string SingleOrderViewDBEndPoint = "SingleOrderViewDB:SingleOrderViewDBEndPoint";
        public static readonly string SingleOrderViewDBName = "SingleOrderViewDB:SingleOrderViewDBName";
    }
}
