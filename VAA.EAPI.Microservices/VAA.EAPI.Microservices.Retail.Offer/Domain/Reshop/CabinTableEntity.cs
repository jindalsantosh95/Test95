using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop
{
    public class CabinTableEntity : TableEntity
    {
        public string CabinCode { get; set; }
        public string CabinName { get; set; }
    }
}
