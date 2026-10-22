using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Models
{
    public class TtlEnabledCityDbEntity : TableEntity
    {
        public string RegionName { get; set; }
        public string CountryCode { get; set; }
        public string AirportCode { get; set; }
        public string LocalCurrencyCode { get; set; }
        public string CityCode { get; set; }

    }
}
