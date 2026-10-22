using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public class CustomerOrderDetails : CosmosDocument
    {
        public string OrderID { get; set; }
        public string CustomerID { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string CreatedOn { get; set; }
        public int TimeToLive { get; set; }
    }
}
