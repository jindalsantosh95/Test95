using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Models
{
    public class BspMarketCurrencyDbEntity : TableEntity
    {
        public string BSPMarket { get; set; }
        public string CountryCode { get; set; }
        public string LocalCurrencyCode { get; set; }
        public string DualCurrencyCode { get; set; }
    }
}
