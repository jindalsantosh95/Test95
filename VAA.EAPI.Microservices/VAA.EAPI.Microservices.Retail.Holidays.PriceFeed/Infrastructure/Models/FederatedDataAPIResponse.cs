namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Models
{
    public class FederatedDataAPIResponse
    {
        public IEnumerable<Holiday> Holidays { get; set; }
        public IEnumerable<Facet> Facet { get; set; }
    }
}
