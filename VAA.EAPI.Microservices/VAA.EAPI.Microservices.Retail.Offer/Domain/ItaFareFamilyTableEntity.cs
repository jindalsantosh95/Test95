using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class ItaFareFamilyTableEntity : TableEntity
    {
        public string VaaProduct { get; set; }
        public string ItaFareFamily { get; set; }
    }
}
