
using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class ProductDetails : TableEntity
    {
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string ProductDescription { get; set; }
        public string ProductCategory { get; set; }
        public string CategoryCode { get; set; }
    }
}
