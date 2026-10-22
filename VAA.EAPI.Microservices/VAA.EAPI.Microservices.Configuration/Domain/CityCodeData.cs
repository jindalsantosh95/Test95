using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Domain
{
    public class CityCodeData
    {
        public string CityCode { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string NDCCityCodeLocation { get; set; }
        public string BSPMarket { get; set; }

        public static explicit operator CityCodeData(CityCodeDbEntity  cityCodeDbEntity)
        {
            return new CityCodeData()
            {
                CountryCode = cityCodeDbEntity.CountryCode,
                CityCode = cityCodeDbEntity.CityCode,
                NDCCityCodeLocation = cityCodeDbEntity.NDCCityCodeLocation,
                BSPMarket = cityCodeDbEntity.BSPMarket,
                CountryName = cityCodeDbEntity.CountryName

            };
        }
    }
}
