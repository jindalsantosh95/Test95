using Microsoft.Azure.Cosmos.Table;
namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class ReferenceProductCodeDataEntity : TableEntity
    {
        public string ProductName { get; set; }
    }
}
