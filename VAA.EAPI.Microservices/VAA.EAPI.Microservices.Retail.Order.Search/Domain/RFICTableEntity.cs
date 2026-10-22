using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class RFICTableEntity : TableEntity
    {
        public string Code { get; set; }
        public string SubCode { get; set; }
    }
}
