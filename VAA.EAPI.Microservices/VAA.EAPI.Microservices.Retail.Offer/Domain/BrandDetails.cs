using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class BrandDetails : TableEntity
    {
        public string VaaProduct { get; set; }
        public string ItaFareFamily { get; set; }
    }
}
