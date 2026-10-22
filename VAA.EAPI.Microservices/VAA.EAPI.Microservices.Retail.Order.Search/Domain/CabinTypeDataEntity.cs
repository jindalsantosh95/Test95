using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class CabinTypeDataEntity : TableEntity
    {
        public string CabinCode { get; set; }
        public string CabinName { get; set; }
    }
}
