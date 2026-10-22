using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Domain
{
    public class TtlEnabledCityData
    {
        public string RegionName { get; set; }
        public string CountryCode { get; set; }
        public string AirportCode { get; set; }
        public string LocalCurrencyCode { get; set; }
        public string CityCode { get; set; }

        public static explicit operator TtlEnabledCityData(TtlEnabledCityDbEntity entity)
        {
            return new TtlEnabledCityData()
            {
                RegionName = entity.RegionName,
                CountryCode = entity.CountryCode,
                AirportCode = entity.AirportCode,
                LocalCurrencyCode = entity.LocalCurrencyCode,
                CityCode = entity.CityCode
            };
        }
    }
}
