using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class SegmentStatusCodeDataEntity : TableEntity
    {
        public string StatusCode { get; set; }
        public string Description { get; set; }
    }
}
