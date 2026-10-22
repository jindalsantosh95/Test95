using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Models
{
    public class CityCodeDbEntity : TableEntity
    {
        public string CityCode { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string NDCCityCodeLocation { get; set; }
        public string BSPMarket { get; set; }
    }
}
