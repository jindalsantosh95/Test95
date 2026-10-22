using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop
{
    public class PriceClassTableEntity : TableEntity
    {
        public string Brand { get; set; }
        public string ProductName { get; set; }
        public string CabinCode { get; set; }
        public string CabinName { get; set; }
    }
}
