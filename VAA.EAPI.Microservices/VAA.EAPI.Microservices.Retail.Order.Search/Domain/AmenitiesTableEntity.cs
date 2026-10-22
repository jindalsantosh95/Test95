using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class AmenitiesTableEntity : TableEntity
    {
        public string Cabins { get; set; }
        public string VAACabins { get; set; }
        public string PriceClass { get; set; }
        public string ProductComponentName { get; set; }
        public string OperatingCarrier { get; set; }
    }
}
